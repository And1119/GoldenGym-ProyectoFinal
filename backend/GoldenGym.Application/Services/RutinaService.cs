using GoldenGym.Application.Common;
using GoldenGym.Application.DTOs;
using GoldenGym.Application.Interfaces;
using GoldenGym.Domain.Entities;
using GoldenGym.Domain.Exceptions;

namespace GoldenGym.Application.Services;

/// <summary>
/// Rutinas de entrenamiento. Entrenador/Administrador las crean y asignan a un cliente.
/// Un entrenador solo puede modificar las rutinas que él creó. El cliente solo ve sus rutinas activas.
/// </summary>
public class RutinaService
{
    private readonly IRutinaRepository _rutinas;
    private readonly IUsuarioRepository _usuarios;
    private readonly IEjercicioRepository _ejercicios;
    private readonly IFechaProvider _fecha;
    private readonly IUnitOfWork _uow;

    public RutinaService(IRutinaRepository rutinas, IUsuarioRepository usuarios, IEjercicioRepository ejercicios,
        IFechaProvider fecha, IUnitOfWork uow)
    {
        _rutinas = rutinas;
        _usuarios = usuarios;
        _ejercicios = ejercicios;
        _fecha = fecha;
        _uow = uow;
    }

    public async Task<List<RutinaDto>> ListarAsync(UsuarioActual actual, int? idCliente, int? idEntrenador, string? buscar, string? nivel, bool incluirInactivas, CancellationToken ct = default)
    {
        var soloActivas = !incluirInactivas;
        if (actual.EsCliente)
        {
            idCliente = actual.IdUsuario;
            soloActivas = true;
        }
        var lista = await _rutinas.ListarAsync(idCliente, idEntrenador, Texto.Limpiar(buscar), Texto.Limpiar(nivel), soloActivas, ct);
        return lista.Select(r => r.ToDto(incluirEjercicios: false)).ToList();
    }

    public async Task<RutinaDto> ObtenerAsync(UsuarioActual actual, int id, CancellationToken ct = default)
    {
        var r = await _rutinas.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("La rutina no existe.");
        if (actual.EsCliente && r.IdCliente != actual.IdUsuario)
            throw new ForbiddenException("No tienes permiso para ver esta rutina.");
        return r.ToDto(incluirEjercicios: true);
    }

    public async Task<RutinaDto> CrearAsync(UsuarioActual actual, RutinaRequest req, CancellationToken ct = default)
    {
        ValidarNivel(req.Nivel);
        await ValidarClienteAsync(req.IdCliente, ct);
        var detalle = await ConstruirDetalleAsync(req.Ejercicios, ct);

        var rutina = new Rutina
        {
            Nombre = req.Nombre.Trim(),
            Objetivo = Texto.Limpiar(req.Objetivo),
            Nivel = req.Nivel,
            IdCliente = req.IdCliente,
            IdEntrenador = actual.IdUsuario,
            FechaCreacion = _fecha.Ahora,
            Activa = req.Activa
        };
        foreach (var d in detalle) rutina.Ejercicios.Add(d);

        await _rutinas.AgregarAsync(rutina, ct);
        await _uow.GuardarCambiosAsync(ct);
        return (await _rutinas.ObtenerPorIdAsync(rutina.IdRutina, ct))!.ToDto(incluirEjercicios: true);
    }

    public async Task<RutinaDto> ActualizarAsync(UsuarioActual actual, int id, RutinaRequest req, CancellationToken ct = default)
    {
        var rutina = await _rutinas.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("La rutina no existe.");
        VerificarPropietario(actual, rutina);
        ValidarNivel(req.Nivel);
        if (rutina.IdCliente != req.IdCliente) await ValidarClienteAsync(req.IdCliente, ct);
        var nuevos = await ConstruirDetalleAsync(req.Ejercicios, ct);

        rutina.Nombre = req.Nombre.Trim();
        rutina.Objetivo = Texto.Limpiar(req.Objetivo);
        rutina.Nivel = req.Nivel;
        rutina.IdCliente = req.IdCliente;
        rutina.Activa = req.Activa;

        // Sincroniza el detalle: actualiza los existentes, quita los que ya no están y agrega los nuevos.
        foreach (var existente in rutina.Ejercicios.ToList())
        {
            var nuevo = nuevos.FirstOrDefault(n => n.IdEjercicio == existente.IdEjercicio);
            if (nuevo is null)
            {
                rutina.Ejercicios.Remove(existente);
                continue;
            }
            existente.Series = nuevo.Series;
            existente.Repeticiones = nuevo.Repeticiones;
            existente.DescansoSeg = nuevo.DescansoSeg;
            existente.Orden = nuevo.Orden;
        }
        foreach (var nuevo in nuevos.Where(n => rutina.Ejercicios.All(e => e.IdEjercicio != n.IdEjercicio)))
            rutina.Ejercicios.Add(nuevo);

        await _uow.GuardarCambiosAsync(ct);
        return (await _rutinas.ObtenerPorIdAsync(id, ct))!.ToDto(incluirEjercicios: true);
    }

    /// <summary>RF-08: desactivación lógica.</summary>
    public async Task DesactivarAsync(UsuarioActual actual, int id, CancellationToken ct = default)
    {
        var rutina = await _rutinas.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("La rutina no existe.");
        VerificarPropietario(actual, rutina);
        rutina.Activa = false;
        await _uow.GuardarCambiosAsync(ct);
    }

    private static void VerificarPropietario(UsuarioActual actual, Rutina rutina)
    {
        if (actual.EsEntrenador && rutina.IdEntrenador != actual.IdUsuario)
            throw new ForbiddenException("Solo puedes modificar las rutinas que tú creaste.");
    }

    private static void ValidarNivel(string nivel)
    {
        if (!NivelRutina.Todos.Contains(nivel))
            throw new BusinessRuleException($"Nivel inválido. Valores permitidos: {string.Join(", ", NivelRutina.Todos)}.");
    }

    private async Task ValidarClienteAsync(int idCliente, CancellationToken ct)
    {
        var cliente = await _usuarios.ObtenerPorIdAsync(idCliente, ct) ?? throw new NotFoundException("El cliente no existe.");
        if (cliente.Rol?.Nombre != RolNombres.Cliente) throw new BusinessRuleException("Las rutinas solo se asignan a usuarios con rol Cliente.");
        if (!cliente.Activo) throw new BusinessRuleException("El cliente está inactivo.");
    }

    private async Task<List<RutinaEjercicio>> ConstruirDetalleAsync(List<RutinaEjercicioRequest> items, CancellationToken ct)
    {
        if (items.Count == 0) throw new BusinessRuleException("La rutina debe tener al menos un ejercicio.");
        if (items.Select(i => i.IdEjercicio).Distinct().Count() != items.Count)
            throw new BusinessRuleException("Un ejercicio no puede repetirse dentro de la misma rutina.");

        var ejercicios = await _ejercicios.ObtenerPorIdsAsync(items.Select(i => i.IdEjercicio), ct);
        foreach (var item in items)
        {
            var e = ejercicios.FirstOrDefault(x => x.IdEjercicio == item.IdEjercicio)
                    ?? throw new NotFoundException($"El ejercicio con Id {item.IdEjercicio} no existe.");
            if (!e.Activo) throw new BusinessRuleException($"El ejercicio '{e.Nombre}' está inactivo y no se puede asignar.");
        }

        return items.Select(i => new RutinaEjercicio
        {
            IdEjercicio = i.IdEjercicio,
            Series = i.Series,
            Repeticiones = i.Repeticiones,
            DescansoSeg = i.DescansoSeg,
            Orden = i.Orden
        }).ToList();
    }
}
