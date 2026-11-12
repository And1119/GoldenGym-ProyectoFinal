using GoldenGym.Application.Common;
using GoldenGym.Application.DTOs;
using GoldenGym.Application.Interfaces;
using GoldenGym.Domain.Entities;
using GoldenGym.Domain.Exceptions;

namespace GoldenGym.Application.Services;

/// <summary>Catálogo de planes (tarifas). Solo el Administrador los modifica.</summary>
public class PlanService
{
    private readonly IPlanRepository _planes;
    private readonly IUnitOfWork _uow;

    public PlanService(IPlanRepository planes, IUnitOfWork uow)
    {
        _planes = planes;
        _uow = uow;
    }

    public async Task<List<PlanDto>> ListarAsync(UsuarioActual? actual, string? buscar, bool incluirInactivos, CancellationToken ct = default)
    {
        // Solo el administrador puede ver planes inactivos.
        var soloActivos = !(incluirInactivos && actual is { EsAdmin: true });
        var lista = await _planes.ListarAsync(Texto.Limpiar(buscar), soloActivos, ct);
        return lista.Select(p => p.ToDto()).ToList();
    }

    public async Task<PlanDto> ObtenerAsync(int id, CancellationToken ct = default) =>
        (await _planes.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("El plan no existe.")).ToDto();

    public async Task<PlanDto> CrearAsync(PlanRequest req, CancellationToken ct = default)
    {
        var nombre = req.Nombre.Trim();
        if (await _planes.ExisteNombreAsync(nombre, null, ct)) throw new ConflictException("Ya existe un plan con ese nombre.");
        var plan = new Plan
        {
            Nombre = nombre,
            Descripcion = Texto.Limpiar(req.Descripcion),
            DuracionDias = req.DuracionDias,
            Precio = req.Precio,
            Activo = req.Activo
        };
        await _planes.AgregarAsync(plan, ct);
        await _uow.GuardarCambiosAsync(ct);
        return plan.ToDto();
    }

    public async Task<PlanDto> ActualizarAsync(int id, PlanRequest req, CancellationToken ct = default)
    {
        var plan = await _planes.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("El plan no existe.");
        var nombre = req.Nombre.Trim();
        if (await _planes.ExisteNombreAsync(nombre, id, ct)) throw new ConflictException("Ya existe un plan con ese nombre.");
        // Cambiar precio o duración no afecta membresías ya vendidas: guardan su propio precio y fechas.
        plan.Nombre = nombre;
        plan.Descripcion = Texto.Limpiar(req.Descripcion);
        plan.DuracionDias = req.DuracionDias;
        plan.Precio = req.Precio;
        plan.Activo = req.Activo;
        await _uow.GuardarCambiosAsync(ct);
        return plan.ToDto();
    }

    /// <summary>RF-08: desactivación lógica (el plan puede tener membresías asociadas).</summary>
    public async Task DesactivarAsync(int id, CancellationToken ct = default)
    {
        var plan = await _planes.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("El plan no existe.");
        plan.Activo = false;
        await _uow.GuardarCambiosAsync(ct);
    }
}
