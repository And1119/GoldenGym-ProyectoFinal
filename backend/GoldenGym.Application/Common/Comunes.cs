using GoldenGym.Domain.Entities;

namespace GoldenGym.Application.Common;

/// <summary>Usuario autenticado, construido a partir de los claims del JWT.</summary>
public record UsuarioActual(int IdUsuario, string Rol)
{
    public bool EsAdmin => Rol == RolNombres.Administrador;
    public bool EsEntrenador => Rol == RolNombres.Entrenador;
    public bool EsCliente => Rol == RolNombres.Cliente;
    /// <summary>Administrador o Entrenador (personal del gimnasio).</summary>
    public bool EsPersonal => EsAdmin || EsEntrenador;
}

/// <summary>Abstrae la fecha/hora actual para poder probar reglas que dependen del tiempo.</summary>
public interface IFechaProvider
{
    DateTime Ahora { get; }
    DateOnly Hoy { get; }
}

public class FechaProviderSistema : IFechaProvider
{
    public DateTime Ahora => DateTime.Now;
    public DateOnly Hoy => DateOnly.FromDateTime(DateTime.Now);
}

public static class Texto
{
    /// <summary>Normaliza un texto opcional: recorta espacios y convierte cadenas vacías en null.</summary>
    public static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    public static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();
}
