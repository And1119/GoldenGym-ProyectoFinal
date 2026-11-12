using GoldenGym.Application.Common;
using GoldenGym.Application.DTOs;
using GoldenGym.Application.Interfaces;
using GoldenGym.Domain.Entities;
using GoldenGym.Domain.Exceptions;

namespace GoldenGym.Application.Services;

/// <summary>
/// Control de asistencia (entrada / salida).
/// Reglas: solo clientes activos con membresía vigente pueden registrar entrada, y no se permite
/// una segunda entrada el mismo día mientras haya una abierta (sin salida).
/// </summary>
public class AsistenciaService
{
    private readonly IAsistenciaRepository _asistencias;
    private readonly IUsuarioRepository _usuarios;
    private readonly IMembresiaRepository _membresias;
    private readonly IFechaProvider _fecha;
    private readonly IUnitOfWork _uow;

    public AsistenciaService(IAsistenciaRepository asistencias, IUsuarioRepository usuarios, IMembresiaRepository membresias,
        IFechaProvider fecha, IUnitOfWork uow)
    {
        _asistencias = asistencias;
        _usuarios = usuarios;
        _membresias = membresias;
        _fecha = fecha;
        _uow = uow;
    }

    public async Task<List<AsistenciaDto>> ListarAsync(UsuarioActual actual, int? idUsuario, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        if (actual.EsCliente) idUsuario = actual.IdUsuario;
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            throw new BusinessRuleException("La fecha 'desde' no puede ser mayor que la fecha 'hasta'.");

        var lista = await _asistencias.ListarAsync(idUsuario,
            desde?.ToDateTime(TimeOnly.MinValue),
            hasta?.ToDateTime(TimeOnly.MaxValue), ct);
        return lista.Select(a => a.ToDto()).ToList();
    }

    public async Task<AsistenciaDto> ObtenerAsync(UsuarioActual actual, int id, CancellationToken ct = default)
    {
        var a = await _asistencias.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("La asistencia no existe.");
        if (actual.EsCliente && a.IdUsuario != actual.IdUsuario)
            throw new ForbiddenException("No tienes permiso para ver esta asistencia.");
        return a.ToDto();
    }

    public async Task<AsistenciaDto> RegistrarEntradaAsync(UsuarioActual actual, AsistenciaEntradaRequest req, CancellationToken ct = default)
    {
        var idCliente = actual.EsCliente ? actual.IdUsuario
            : req.IdUsuario ?? throw new BusinessRuleException("Debe indicar el cliente que ingresa.");

        var cliente = await _usuarios.ObtenerPorIdAsync(idCliente, ct) ?? throw new NotFoundException("El cliente no existe.");
        if (cliente.Rol?.Nombre != RolNombres.Cliente) throw new BusinessRuleException("Solo se registra asistencia de clientes.");
        if (!cliente.Activo) throw new BusinessRuleException("El cliente está inactivo.");

        var hoy = _fecha.Hoy;
        var membresias = await _membresias.ListarActivasDeUsuarioAsync(idCliente, ct);
        if (!membresias.Any(m => m.EstaVigente(hoy)))
            throw new BusinessRuleException("El cliente no tiene una membresía vigente. Debe adquirir o renovar un plan.");

        if (await _asistencias.ObtenerAbiertaDelDiaAsync(idCliente, hoy, ct) is not null)
            throw new BusinessRuleException("El cliente ya tiene una entrada abierta hoy. Registra primero la salida.");

        var asistencia = new Asistencia
        {
            IdUsuario = idCliente,
            FechaEntrada = _fecha.Ahora,
            IdRegistradoPor = actual.IdUsuario
        };
        await _asistencias.AgregarAsync(asistencia, ct);
        await _uow.GuardarCambiosAsync(ct);
        return (await _asistencias.ObtenerPorIdAsync(asistencia.IdAsistencia, ct) ?? asistencia).ToDto();
    }

    public async Task<AsistenciaDto> RegistrarSalidaAsync(UsuarioActual actual, int id, CancellationToken ct = default)
    {
        var a = await _asistencias.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("La asistencia no existe.");
        if (actual.EsCliente && a.IdUsuario != actual.IdUsuario)
            throw new ForbiddenException("No puedes registrar la salida de otro cliente.");
        if (a.FechaSalida.HasValue) throw new BusinessRuleException("Esta asistencia ya tiene registrada la salida.");

        var ahora = _fecha.Ahora;
        a.FechaSalida = ahora < a.FechaEntrada ? a.FechaEntrada : ahora;
        await _uow.GuardarCambiosAsync(ct);
        return a.ToDto();
    }

    /// <summary>Solo administrador: corrige registros erróneos.</summary>
    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var a = await _asistencias.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("La asistencia no existe.");
        _asistencias.Eliminar(a);
        await _uow.GuardarCambiosAsync(ct);
    }
}
