using GoldenGym.Api.Extensions;
using GoldenGym.Application.DTOs;
using GoldenGym.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldenGym.Api.Controllers;

/// <summary>Usuarios. Listar/consultar: Administrador y Entrenador (solo clientes). Crear/editar/desactivar: Administrador.</summary>
[Route("api/usuarios")]
[Authorize(Policy = Politicas.Personal)]
public class UsuariosController : ApiControllerBase
{
    private readonly UsuarioService _service;
    public UsuariosController(UsuarioService service) => _service = service;

    /// <summary>RF-09: filtros por texto (nombre, correo, documento), rol y estado.</summary>
    [HttpGet]
    public Task<List<UsuarioDto>> Listar([FromQuery] string? buscar, [FromQuery] string? rol, [FromQuery] bool? activo, CancellationToken ct) =>
        _service.ListarAsync(Actual, buscar, rol, activo, ct);

    [HttpGet("{id:int}")]
    public Task<UsuarioDto> Obtener(int id, CancellationToken ct) => _service.ObtenerAsync(Actual, id, ct);

    [HttpPost]
    [Authorize(Policy = Politicas.SoloAdmin)]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<UsuarioDto>> Crear(UsuarioCreateRequest req, CancellationToken ct)
    {
        var dto = await _service.CrearAsync(req, ct);
        return CreatedAtAction(nameof(Obtener), new { id = dto.IdUsuario }, dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.SoloAdmin)]
    public Task<UsuarioDto> Actualizar(int id, UsuarioUpdateRequest req, CancellationToken ct) =>
        _service.ActualizarAsync(Actual, id, req, ct);

    /// <summary>RF-08: desactiva el usuario (no lo borra).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.SoloAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Desactivar(int id, CancellationToken ct)
    {
        await _service.DesactivarAsync(Actual, id, ct);
        return NoContent();
    }
}

/// <summary>Planes / tarifas. Consulta pública; modificación solo Administrador.</summary>
[Route("api/planes")]
public class PlanesController : ApiControllerBase
{
    private readonly PlanService _service;
    public PlanesController(PlanService service) => _service = service;

    [HttpGet]
    [AllowAnonymous]
    public Task<List<PlanDto>> Listar([FromQuery] string? buscar, [FromQuery] bool incluirInactivos, CancellationToken ct) =>
        _service.ListarAsync(User.ObtenerUsuarioActualOpcional(), buscar, incluirInactivos, ct);

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public Task<PlanDto> Obtener(int id, CancellationToken ct) => _service.ObtenerAsync(id, ct);

    [HttpPost]
    [Authorize(Policy = Politicas.SoloAdmin)]
    [ProducesResponseType(typeof(PlanDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PlanDto>> Crear(PlanRequest req, CancellationToken ct)
    {
        var dto = await _service.CrearAsync(req, ct);
        return CreatedAtAction(nameof(Obtener), new { id = dto.IdPlan }, dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.SoloAdmin)]
    public Task<PlanDto> Actualizar(int id, PlanRequest req, CancellationToken ct) => _service.ActualizarAsync(id, req, ct);

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.SoloAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Desactivar(int id, CancellationToken ct)
    {
        await _service.DesactivarAsync(id, ct);
        return NoContent();
    }
}

/// <summary>Membresías. Cliente: las suyas y compra de plan. Administrador: gestión completa.</summary>
[Route("api/membresias")]
[Authorize(Policy = Politicas.Autenticado)]
public class MembresiasController : ApiControllerBase
{
    private readonly MembresiaService _service;
    public MembresiasController(MembresiaService service) => _service = service;

    /// <summary>RF-09: filtros por cliente, plan, estado (Vigente, Pendiente, Vencida, Cancelada) y texto.</summary>
    [HttpGet]
    public Task<List<MembresiaDto>> Listar([FromQuery] int? idUsuario, [FromQuery] int? idPlan, [FromQuery] string? estado,
        [FromQuery] string? buscar, CancellationToken ct) =>
        _service.ListarAsync(Actual, idUsuario, idPlan, estado, buscar, ct);

    [HttpGet("{id:int}")]
    public Task<MembresiaDto> Obtener(int id, CancellationToken ct) => _service.ObtenerAsync(Actual, id, ct);

    [HttpPost]
    [ProducesResponseType(typeof(MembresiaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<MembresiaDto>> Crear(MembresiaRequest req, CancellationToken ct)
    {
        var dto = await _service.CrearAsync(Actual, req, ct);
        return CreatedAtAction(nameof(Obtener), new { id = dto.IdMembresia }, dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.SoloAdmin)]
    public Task<MembresiaDto> Actualizar(int id, MembresiaRequest req, CancellationToken ct) => _service.ActualizarAsync(id, req, ct);

    /// <summary>RF-08: cancela la membresía (no la borra).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.SoloAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancelar(int id, CancellationToken ct)
    {
        await _service.CancelarAsync(id, ct);
        return NoContent();
    }
}
