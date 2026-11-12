using GoldenGym.Application.Common;
using GoldenGym.Application.DTOs;
using GoldenGym.Application.Interfaces;
using GoldenGym.Domain.Entities;
using GoldenGym.Domain.Exceptions;

namespace GoldenGym.Application.Services;

/// <summary>Seguimiento del progreso físico. Entrenador/Administrador registran; el cliente consulta el suyo.</summary>
public class ProgresoService
{
    private readonly IProgresoRepository _progresos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IFechaProvider _fecha;
    private readonly IUnitOfWork _uow;

    public ProgresoService(IProgresoRepository progresos, IUsuarioRepository usuarios, IFechaProvider fecha, IUnitOfWork uow)
    {
        _progresos = progresos;
        _usuarios = usuarios;
        _fecha = fecha;
        _uow = uow;
    }

    public async Task<List<ProgresoDto>> ListarAsync(UsuarioActual actual, int? idUsuario, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        if (actual.EsCliente) idUsuario = actual.IdUsuario;
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            throw new BusinessRuleException("La fecha 'desde' no puede ser mayor que la fecha 'hasta'.");
        var lista = await _progresos.ListarAsync(idUsuario, desde, hasta, ct);
        return lista.Select(p => p.ToDto()).ToList();
    }

    public async Task<ProgresoDto> ObtenerAsync(UsuarioActual actual, int id, CancellationToken ct = default)
    {
        var p = await _progresos.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("El registro de progreso no existe.");
        if (actual.EsCliente && p.IdUsuario != actual.IdUsuario)
            throw new ForbiddenException("No tienes permiso para ver este registro.");
        return p.ToDto();
    }

    public async Task<ProgresoDto> CrearAsync(UsuarioActual actual, ProgresoRequest req, CancellationToken ct = default)
    {
        await ValidarAsync(req, ct);
        var p = new Progreso
        {
            IdUsuario = req.IdUsuario,
            IdEntrenador = actual.IdUsuario,
            Fecha = req.Fecha!.Value,
            PesoKg = req.PesoKg,
            EstaturaCm = req.EstaturaCm,
            GrasaCorporal = req.GrasaCorporal,
            Observaciones = Texto.Limpiar(req.Observaciones)
        };
        await _progresos.AgregarAsync(p, ct);
        await _uow.GuardarCambiosAsync(ct);
        return (await _progresos.ObtenerPorIdAsync(p.IdProgreso, ct) ?? p).ToDto();
    }

    public async Task<ProgresoDto> ActualizarAsync(int id, ProgresoRequest req, CancellationToken ct = default)
    {
        var p = await _progresos.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("El registro de progreso no existe.");
        await ValidarAsync(req, ct);
        p.IdUsuario = req.IdUsuario;
        p.Fecha = req.Fecha!.Value;
        p.PesoKg = req.PesoKg;
        p.EstaturaCm = req.EstaturaCm;
        p.GrasaCorporal = req.GrasaCorporal;
        p.Observaciones = Texto.Limpiar(req.Observaciones);
        await _uow.GuardarCambiosAsync(ct);
        return (await _progresos.ObtenerPorIdAsync(id, ct) ?? p).ToDto();
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var p = await _progresos.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("El registro de progreso no existe.");
        _progresos.Eliminar(p);
        await _uow.GuardarCambiosAsync(ct);
    }

    private async Task ValidarAsync(ProgresoRequest req, CancellationToken ct)
    {
        if (req.Fecha > _fecha.Hoy) throw new BusinessRuleException("La fecha del registro no puede ser futura.");
        var cliente = await _usuarios.ObtenerPorIdAsync(req.IdUsuario, ct) ?? throw new NotFoundException("El cliente no existe.");
        if (cliente.Rol?.Nombre != RolNombres.Cliente) throw new BusinessRuleException("El progreso solo se registra a usuarios con rol Cliente.");
    }
}
