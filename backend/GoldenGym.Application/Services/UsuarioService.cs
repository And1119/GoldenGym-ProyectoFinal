using GoldenGym.Application.Common;
using GoldenGym.Application.DTOs;
using GoldenGym.Application.Interfaces;
using GoldenGym.Domain.Entities;
using GoldenGym.Domain.Exceptions;

namespace GoldenGym.Application.Services;

/// <summary>Gestión de usuarios. Administrador: todo. Entrenador: solo consulta clientes.</summary>
public class UsuarioService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly IUnitOfWork _uow;

    public UsuarioService(IUsuarioRepository usuarios, IPasswordHasher hasher, IUnitOfWork uow)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _uow = uow;
    }

    public async Task<List<UsuarioDto>> ListarAsync(UsuarioActual actual, string? buscar, string? rol, bool? activo, CancellationToken ct = default)
    {
        if (actual.EsCliente) throw new ForbiddenException("No tienes permiso para consultar usuarios.");
        // El entrenador solo puede ver clientes.
        if (actual.EsEntrenador) rol = RolNombres.Cliente;
        var lista = await _usuarios.ListarAsync(Texto.Limpiar(buscar), Texto.Limpiar(rol), activo, ct);
        return lista.Select(u => u.ToDto()).ToList();
    }

    public async Task<UsuarioDto> ObtenerAsync(UsuarioActual actual, int id, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("El usuario no existe.");
        var puedeVer = actual.EsAdmin
                       || actual.IdUsuario == id
                       || (actual.EsEntrenador && usuario.Rol?.Nombre == RolNombres.Cliente);
        if (!puedeVer) throw new ForbiddenException("No tienes permiso para ver este usuario.");
        return usuario.ToDto();
    }

    public async Task<UsuarioDto> CrearAsync(UsuarioCreateRequest req, CancellationToken ct = default)
    {
        ValidarRol(req.Rol);
        var usuario = await AuthService.CrearUsuarioAsync(req, req.Rol, _usuarios, _hasher, ct);
        await _usuarios.AgregarAsync(usuario, ct);
        await _uow.GuardarCambiosAsync(ct);
        return usuario.ToDto();
    }

    public async Task<UsuarioDto> ActualizarAsync(UsuarioActual admin, int id, UsuarioUpdateRequest req, CancellationToken ct = default)
    {
        ValidarRol(req.Rol);
        var usuario = await _usuarios.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("El usuario no existe.");

        if (admin.IdUsuario == id && (!req.Activo || req.Rol != RolNombres.Administrador))
            throw new BusinessRuleException("No puedes desactivarte ni quitarte el rol de Administrador a ti mismo.");

        var documento = Texto.Limpiar(req.Documento);
        if (documento is not null && await _usuarios.ExisteDocumentoAsync(documento, id, ct))
            throw new ConflictException("El documento ya está registrado por otro usuario.");

        var rol = await _usuarios.ObtenerRolPorNombreAsync(req.Rol, ct) ?? throw new NotFoundException("El rol no existe.");

        usuario.Nombres = req.Nombres.Trim();
        usuario.Apellidos = req.Apellidos.Trim();
        usuario.Telefono = Texto.Limpiar(req.Telefono);
        usuario.Documento = documento;
        usuario.FechaNacimiento = req.FechaNacimiento;
        usuario.IdRol = rol.IdRol;
        usuario.Rol = rol;
        usuario.Activo = req.Activo;
        await _uow.GuardarCambiosAsync(ct);
        return usuario.ToDto();
    }

    /// <summary>RF-08: los usuarios no se borran físicamente (tienen historial), se desactivan.</summary>
    public async Task DesactivarAsync(UsuarioActual admin, int id, CancellationToken ct = default)
    {
        if (admin.IdUsuario == id) throw new BusinessRuleException("No puedes desactivar tu propia cuenta.");
        var usuario = await _usuarios.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("El usuario no existe.");
        usuario.Activo = false;
        await _uow.GuardarCambiosAsync(ct);
    }

    private static void ValidarRol(string rol)
    {
        if (!RolNombres.Todos.Contains(rol))
            throw new BusinessRuleException($"Rol inválido. Valores permitidos: {string.Join(", ", RolNombres.Todos)}.");
    }
}
