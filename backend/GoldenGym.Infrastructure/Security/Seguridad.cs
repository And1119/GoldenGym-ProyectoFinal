using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GoldenGym.Application.Interfaces;
using GoldenGym.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GoldenGym.Infrastructure.Security;

/// <summary>Configuración JWT (sección "Jwt" de appsettings.json).</summary>
public class JwtOptions
{
    public const string Seccion = "Jwt";
    public string Issuer { get; set; } = "GoldenGym.Api";
    public string Audience { get; set; } = "GoldenGym.Android";
    /// <summary>Clave secreta HMAC-SHA256. Mínimo 32 caracteres.</summary>
    public string Key { get; set; } = string.Empty;
    public int ExpiracionMinutos { get; set; } = 120;
}

/// <summary>Hash seguro de contraseñas con BCrypt (work factor 11, sal aleatoria incluida en el hash).</summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verificar(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (Exception ex) when (ex is BCrypt.Net.SaltParseException or ArgumentException)
        {
            return false; // hash con formato inválido en BD
        }
    }
}

/// <summary>Genera tokens JWT firmados con HS256 que incluyen el Id y el rol del usuario.</summary>
public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _opciones;

    public JwtTokenService(IOptions<JwtOptions> opciones) => _opciones = opciones.Value;

    public (string Token, DateTime ExpiraEnUtc) GenerarToken(Usuario usuario)
    {
        var expira = DateTime.UtcNow.AddMinutes(_opciones.ExpiracionMinutos);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Email, usuario.Correo),
            new(ClaimTypes.Name, usuario.NombreCompleto),
            new(ClaimTypes.Role, usuario.Rol?.Nombre ?? RolNombres.Cliente),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Key)), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opciones.Issuer,
            audience: _opciones.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expira,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
