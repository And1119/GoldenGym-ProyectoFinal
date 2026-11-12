using GoldenGym.Api.Extensions;
using GoldenGym.Application.Common;
using GoldenGym.Application.DTOs;
using GoldenGym.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldenGym.Api.Controllers;

/// <summary>Base de los controladores: documenta los errores comunes y expone el usuario autenticado.</summary>
[ApiController]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public abstract class ApiControllerBase : ControllerBase
{
    protected UsuarioActual Actual => User.ObtenerUsuarioActual();
}

/// <summary>Registro, inicio de sesión y perfil del usuario autenticado.</summary>
[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly AuthService _auth;
    public AuthController(AuthService auth) => _auth = auth;

    /// <summary>RF-01: registro público de clientes (correo único). Devuelve el token para iniciar sesión directamente.</summary>
    [HttpPost("registro")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Registrar(RegistroRequest req, CancellationToken ct)
    {
        var res = await _auth.RegistrarAsync(req, ct);
        return CreatedAtAction(nameof(Perfil), null, res);
    }

    /// <summary>RF-02: inicio de sesión. Devuelve un JWT firmado con el rol del usuario.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public Task<AuthResponse> Login(LoginRequest req, CancellationToken ct) => _auth.LoginAsync(req, ct);

    /// <summary>RF-04: perfil del usuario autenticado.</summary>
    [HttpGet("perfil")]
    [Authorize(Policy = Politicas.Autenticado)]
    public Task<UsuarioDto> Perfil(CancellationToken ct) => _auth.ObtenerPerfilAsync(Actual.IdUsuario, ct);

    [HttpPut("perfil")]
    [Authorize(Policy = Politicas.Autenticado)]
    public Task<UsuarioDto> ActualizarPerfil(PerfilUpdateRequest req, CancellationToken ct) =>
        _auth.ActualizarPerfilAsync(Actual.IdUsuario, req, ct);

    [HttpPut("password")]
    [Authorize(Policy = Politicas.Autenticado)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CambiarPassword(CambioPasswordRequest req, CancellationToken ct)
    {
        await _auth.CambiarPasswordAsync(Actual.IdUsuario, req, ct);
        return NoContent();
    }
}

/// <summary>Datos de la pantalla Home según el rol.</summary>
[Route("api/dashboard")]
[Authorize(Policy = Politicas.Autenticado)]
public class DashboardController : ApiControllerBase
{
    private readonly DashboardService _service;
    public DashboardController(DashboardService service) => _service = service;

    [HttpGet]
    public Task<DashboardDto> Obtener(CancellationToken ct) => _service.ObtenerAsync(Actual, ct);
}
