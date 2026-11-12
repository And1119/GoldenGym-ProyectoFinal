using GoldenGym.Api.Extensions;
using GoldenGym.Application.DTOs;
using GoldenGym.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldenGym.Api.Controllers;

/// <summary>Catálogo de ejercicios. Consulta: todos. Gestión: Administrador y Entrenador.</summary>
[Route("api/ejercicios")]
[Authorize(Policy = Politicas.Autenticado)]
public class EjerciciosController : ApiControllerBase
{
    private readonly EjercicioService _service;
    public EjerciciosController(EjercicioService service) => _service = service;

    /// <summary>RF-09: filtros por texto y grupo muscular.</summary>
    [HttpGet]
    public Task<List<EjercicioDto>> Listar([FromQuery] string? buscar, [FromQuery] string? grupoMuscular,
        [FromQuery] bool incluirInactivos, CancellationToken ct) =>
        _service.ListarAsync(Actual, buscar, grupoMuscular, incluirInactivos, ct);

    [HttpGet("{id:int}")]
    public Task<EjercicioDto> Obtener(int id, CancellationToken ct) => _service.ObtenerAsync(id, ct);

    [HttpPost]
    [Authorize(Policy = Politicas.Personal)]
    [ProducesResponseType(typeof(EjercicioDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<EjercicioDto>> Crear(EjercicioRequest req, CancellationToken ct)
    {
        var dto = await _service.CrearAsync(req, ct);
        return CreatedAtAction(nameof(Obtener), new { id = dto.IdEjercicio }, dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.Personal)]
    public Task<EjercicioDto> Actualizar(int id, EjercicioRequest req, CancellationToken ct) => _service.ActualizarAsync(id, req, ct);

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.Personal)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Desactivar(int id, CancellationToken ct)
    {
        await _service.DesactivarAsync(id, ct);
        return NoContent();
    }
}

/// <summary>Rutinas. Cliente: consulta las suyas. Entrenador/Administrador: gestión.</summary>
[Route("api/rutinas")]
[Authorize(Policy = Politicas.Autenticado)]
public class RutinasController : ApiControllerBase
{
    private readonly RutinaService _service;
    public RutinasController(RutinaService service) => _service = service;

    /// <summary>RF-09: filtros por texto, nivel, cliente y entrenador.</summary>
    [HttpGet]
    public Task<List<RutinaDto>> Listar([FromQuery] int? idCliente, [FromQuery] int? idEntrenador, [FromQuery] string? buscar,
        [FromQuery] string? nivel, [FromQuery] bool incluirInactivas, CancellationToken ct) =>
        _service.ListarAsync(Actual, idCliente, idEntrenador, buscar, nivel, incluirInactivas, ct);

    /// <summary>RF-10: detalle con la lista de ejercicios.</summary>
    [HttpGet("{id:int}")]
    public Task<RutinaDto> Obtener(int id, CancellationToken ct) => _service.ObtenerAsync(Actual, id, ct);

    [HttpPost]
    [Authorize(Policy = Politicas.Personal)]
    [ProducesResponseType(typeof(RutinaDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<RutinaDto>> Crear(RutinaRequest req, CancellationToken ct)
    {
        var dto = await _service.CrearAsync(Actual, req, ct);
        return CreatedAtAction(nameof(Obtener), new { id = dto.IdRutina }, dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.Personal)]
    public Task<RutinaDto> Actualizar(int id, RutinaRequest req, CancellationToken ct) => _service.ActualizarAsync(Actual, id, req, ct);

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.Personal)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Desactivar(int id, CancellationToken ct)
    {
        await _service.DesactivarAsync(Actual, id, ct);
        return NoContent();
    }
}

/// <summary>Asistencia (entrada / salida).</summary>
[Route("api/asistencias")]
[Authorize(Policy = Politicas.Autenticado)]
public class AsistenciasController : ApiControllerBase
{
    private readonly AsistenciaService _service;
    public AsistenciasController(AsistenciaService service) => _service = service;

    /// <summary>RF-09: filtros por cliente y rango de fechas (yyyy-MM-dd).</summary>
    [HttpGet]
    public Task<List<AsistenciaDto>> Listar([FromQuery] int? idUsuario, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct) =>
        _service.ListarAsync(Actual, idUsuario, desde, hasta, ct);

    [HttpGet("{id:int}")]
    public Task<AsistenciaDto> Obtener(int id, CancellationToken ct) => _service.ObtenerAsync(Actual, id, ct);

    /// <summary>Registra la entrada. Requiere membresía vigente.</summary>
    [HttpPost("entrada")]
    [ProducesResponseType(typeof(AsistenciaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AsistenciaDto>> Entrada(AsistenciaEntradaRequest req, CancellationToken ct)
    {
        var dto = await _service.RegistrarEntradaAsync(Actual, req, ct);
        return CreatedAtAction(nameof(Obtener), new { id = dto.IdAsistencia }, dto);
    }

    [HttpPut("{id:int}/salida")]
    public Task<AsistenciaDto> Salida(int id, CancellationToken ct) => _service.RegistrarSalidaAsync(Actual, id, ct);

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.SoloAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _service.EliminarAsync(id, ct);
        return NoContent();
    }
}

/// <summary>Progreso físico. Cliente: consulta. Entrenador/Administrador: gestión.</summary>
[Route("api/progresos")]
[Authorize(Policy = Politicas.Autenticado)]
public class ProgresosController : ApiControllerBase
{
    private readonly ProgresoService _service;
    public ProgresosController(ProgresoService service) => _service = service;

    [HttpGet]
    public Task<List<ProgresoDto>> Listar([FromQuery] int? idUsuario, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct) =>
        _service.ListarAsync(Actual, idUsuario, desde, hasta, ct);

    [HttpGet("{id:int}")]
    public Task<ProgresoDto> Obtener(int id, CancellationToken ct) => _service.ObtenerAsync(Actual, id, ct);

    [HttpPost]
    [Authorize(Policy = Politicas.Personal)]
    [ProducesResponseType(typeof(ProgresoDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProgresoDto>> Crear(ProgresoRequest req, CancellationToken ct)
    {
        var dto = await _service.CrearAsync(Actual, req, ct);
        return CreatedAtAction(nameof(Obtener), new { id = dto.IdProgreso }, dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.Personal)]
    public Task<ProgresoDto> Actualizar(int id, ProgresoRequest req, CancellationToken ct) => _service.ActualizarAsync(id, req, ct);

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.Personal)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _service.EliminarAsync(id, ct);
        return NoContent();
    }
}
