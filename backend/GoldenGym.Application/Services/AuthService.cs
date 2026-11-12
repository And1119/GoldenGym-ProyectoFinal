using GoldenGym.Application.Common;
using GoldenGym.Application.DTOs;
using GoldenGym.Application.Interfaces;
using GoldenGym.Domain.Entities;
using GoldenGym.Domain.Exceptions;

namespace GoldenGym.Application.Services;

/// <summary>Registro, login (JWT) y gestión del perfil propio. RF-01, RF-02, RF-04.</summary>
public class AuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly IUnitOfWork _uow;

    public AuthService(IUsuarioRepository usuarios, IPasswordHasher hasher, ITokenService tokens, IUnitOfWork uow)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _tokens = tokens;
        _uow = uow;
    }

    /// <summary>Registro público: siempre crea un usuario con rol Cliente y valida correo único.</summary>
    public async Task<AuthResponse> RegistrarAsync(RegistroRequest req, CancellationToken ct = default)
    {
        var usuario = await CrearUsuarioAsync(req, RolNombres.Cliente, _usuarios, _hasher, ct);
        await _usuarios.AgregarAsync(usuario, ct);
        await _uow.GuardarCambiosAsync(ct);
        return GenerarRespuesta(usuario);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObtenerPorCorreoAsync(Texto.NormalizarCorreo(req.Correo), ct);
        // Mismo mensaje para correo inexistente y contraseña errada: no revela qué correos existen.
        if (usuario is null || !_hasher.Verificar(req.Password, usuario.PasswordHash))
            throw new UnauthorizedException("Correo o contraseña incorrectos.");
        if (!usuario.Activo)
            throw new UnauthorizedException("Tu cuenta está inactiva. Comunícate con el administrador del gimnasio.");

        return GenerarRespuesta(usuario);
    }

    public async Task<UsuarioDto> ObtenerPerfilAsync(int idUsuario, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(idUsuario, ct)
                      ?? throw new NotFoundException("El usuario no existe.");
        return usuario.ToDto();
    }

    public async Task<UsuarioDto> ActualizarPerfilAsync(int idUsuario, PerfilUpdateRequest req, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(idUsuario, ct)
                      ?? throw new NotFoundException("El usuario no existe.");
        usuario.Nombres = req.Nombres.Trim();
        usuario.Apellidos = req.Apellidos.Trim();
        usuario.Telefono = Texto.Limpiar(req.Telefono);
        usuario.FechaNacimiento = req.FechaNacimiento;
        await _uow.GuardarCambiosAsync(ct);
        return usuario.ToDto();
    }

    public async Task CambiarPasswordAsync(int idUsuario, CambioPasswordRequest req, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(idUsuario, ct)
                      ?? throw new NotFoundException("El usuario no existe.");
        if (!_hasher.Verificar(req.PasswordActual, usuario.PasswordHash))
            throw new BusinessRuleException("La contraseña actual no es correcta.");
        usuario.PasswordHash = _hasher.Hash(req.PasswordNueva);
        await _uow.GuardarCambiosAsync(ct);
    }

    private AuthResponse GenerarRespuesta(Usuario usuario)
    {
        var (token, expira) = _tokens.GenerarToken(usuario);
        return new AuthResponse(token, expira, usuario.ToDto());
    }

    /// <summary>Construye un usuario validando correo y documento únicos. Lo reutiliza UsuarioService.</summary>
    internal static async Task<Usuario> CrearUsuarioAsync(RegistroRequest req, string nombreRol,
        IUsuarioRepository usuarios, IPasswordHasher hasher, CancellationToken ct)
    {
        var correo = Texto.NormalizarCorreo(req.Correo);
        if (await usuarios.ExisteCorreoAsync(correo, null, ct))
            throw new ConflictException("El correo ya está registrado.");

        var documento = Texto.Limpiar(req.Documento);
        if (documento is not null && await usuarios.ExisteDocumentoAsync(documento, null, ct))
            throw new ConflictException("El documento ya está registrado.");

        var rol = await usuarios.ObtenerRolPorNombreAsync(nombreRol, ct)
                  ?? throw new BusinessRuleException($"El rol '{nombreRol}' no existe.");

        return new Usuario
        {
            IdRol = rol.IdRol,
            Rol = rol,
            Nombres = req.Nombres.Trim(),
            Apellidos = req.Apellidos.Trim(),
            Correo = correo,
            PasswordHash = hasher.Hash(req.Password),
            Telefono = Texto.Limpiar(req.Telefono),
            Documento = documento,
            FechaNacimiento = req.FechaNacimiento,
            Activo = true,
            FechaRegistro = DateTime.Now
        };
    }
}
