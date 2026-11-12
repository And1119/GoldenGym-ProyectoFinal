using System.Security.Claims;
using GoldenGym.Application.Common;
using GoldenGym.Domain.Entities;
using GoldenGym.Domain.Exceptions;

namespace GoldenGym.Api.Extensions;

/// <summary>Nombres de las políticas de autorización por rol (RNF-03).</summary>
public static class Politicas
{
    public const string SoloAdmin = "SoloAdmin";
    /// <summary>Personal del gimnasio: Administrador o Entrenador.</summary>
    public const string Personal = "Personal";
    /// <summary>Cualquier usuario autenticado con uno de los tres roles.</summary>
    public const string Autenticado = "Autenticado";

    public static void Registrar(Microsoft.AspNetCore.Authorization.AuthorizationOptions o)
    {
        o.AddPolicy(SoloAdmin, p => p.RequireRole(RolNombres.Administrador));
        o.AddPolicy(Personal, p => p.RequireRole(RolNombres.Administrador, RolNombres.Entrenador));
        o.AddPolicy(Autenticado, p => p.RequireRole(RolNombres.Todos));
    }
}

public static class ClaimsExtensions
{
    /// <summary>Obtiene el usuario autenticado (Id y rol) a partir de los claims validados del JWT.</summary>
    public static UsuarioActual ObtenerUsuarioActual(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        var rol = user.FindFirstValue(ClaimTypes.Role);
        if (!int.TryParse(id, out var idUsuario) || string.IsNullOrEmpty(rol))
            throw new UnauthorizedException("Token inválido.");
        return new UsuarioActual(idUsuario, rol);
    }

    public static UsuarioActual? ObtenerUsuarioActualOpcional(this ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true ? user.ObtenerUsuarioActual() : null;
}
