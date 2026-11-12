using GoldenGym.Application.Common;
using GoldenGym.Application.DTOs;
using GoldenGym.Application.Interfaces;
using GoldenGym.Domain.Entities;
using GoldenGym.Domain.Exceptions;

namespace GoldenGym.Application.Services;

/// <summary>Catálogo de ejercicios. Administrador y Entrenador lo gestionan; todos lo consultan.</summary>
public class EjercicioService
{
    private readonly IEjercicioRepository _ejercicios;
    private readonly IUnitOfWork _uow;

    public EjercicioService(IEjercicioRepository ejercicios, IUnitOfWork uow)
    {
        _ejercicios = ejercicios;
        _uow = uow;
    }

    public async Task<List<EjercicioDto>> ListarAsync(UsuarioActual actual, string? buscar, string? grupoMuscular, bool incluirInactivos, CancellationToken ct = default)
    {
        var soloActivos = !(incluirInactivos && actual.EsPersonal);
        var lista = await _ejercicios.ListarAsync(Texto.Limpiar(buscar), Texto.Limpiar(grupoMuscular), soloActivos, ct);
        return lista.Select(e => e.ToDto()).ToList();
    }

    public async Task<EjercicioDto> ObtenerAsync(int id, CancellationToken ct = default) =>
        (await _ejercicios.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("El ejercicio no existe.")).ToDto();

    public async Task<EjercicioDto> CrearAsync(EjercicioRequest req, CancellationToken ct = default)
    {
        var nombre = req.Nombre.Trim();
        if (await _ejercicios.ExisteNombreAsync(nombre, null, ct)) throw new ConflictException("Ya existe un ejercicio con ese nombre.");
        var e = new Ejercicio
        {
            Nombre = nombre,
            GrupoMuscular = req.GrupoMuscular.Trim(),
            Descripcion = Texto.Limpiar(req.Descripcion),
            Activo = req.Activo
        };
        await _ejercicios.AgregarAsync(e, ct);
        await _uow.GuardarCambiosAsync(ct);
        return e.ToDto();
    }

    public async Task<EjercicioDto> ActualizarAsync(int id, EjercicioRequest req, CancellationToken ct = default)
    {
        var e = await _ejercicios.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("El ejercicio no existe.");
        var nombre = req.Nombre.Trim();
        if (await _ejercicios.ExisteNombreAsync(nombre, id, ct)) throw new ConflictException("Ya existe un ejercicio con ese nombre.");
        e.Nombre = nombre;
        e.GrupoMuscular = req.GrupoMuscular.Trim();
        e.Descripcion = Texto.Limpiar(req.Descripcion);
        e.Activo = req.Activo;
        await _uow.GuardarCambiosAsync(ct);
        return e.ToDto();
    }

    /// <summary>RF-08: desactivación lógica para no romper las rutinas que ya lo usan.</summary>
    public async Task DesactivarAsync(int id, CancellationToken ct = default)
    {
        var e = await _ejercicios.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("El ejercicio no existe.");
        e.Activo = false;
        await _uow.GuardarCambiosAsync(ct);
    }
}
