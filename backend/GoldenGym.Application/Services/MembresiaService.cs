using GoldenGym.Application.Common;
using GoldenGym.Application.DTOs;
using GoldenGym.Application.Interfaces;
using GoldenGym.Domain.Entities;
using GoldenGym.Domain.Exceptions;

namespace GoldenGym.Application.Services;

/// <summary>
/// Membresías. Administrador: CRUD completo. Cliente: consulta las suyas y puede adquirir un plan (compra simulada).
/// Reglas: el plan debe estar activo, el usuario debe ser Cliente activo y no se permiten membresías con fechas cruzadas.
/// </summary>
public class MembresiaService
{
    private readonly IMembresiaRepository _membresias;
    private readonly IUsuarioRepository _usuarios;
    private readonly IPlanRepository _planes;
    private readonly IFechaProvider _fecha;
    private readonly IUnitOfWork _uow;

    public MembresiaService(IMembresiaRepository membresias, IUsuarioRepository usuarios, IPlanRepository planes,
        IFechaProvider fecha, IUnitOfWork uow)
    {
        _membresias = membresias;
        _usuarios = usuarios;
        _planes = planes;
        _fecha = fecha;
        _uow = uow;
    }

    public async Task<List<MembresiaDto>> ListarAsync(UsuarioActual actual, int? idUsuario, int? idPlan, string? estado, string? buscar, CancellationToken ct = default)
    {
        if (actual.EsCliente) idUsuario = actual.IdUsuario; // el cliente solo ve las suyas
        var hoy = _fecha.Hoy;
        var lista = (await _membresias.ListarAsync(idUsuario, idPlan, Texto.Limpiar(buscar), ct))
            .Select(m => m.ToDto(hoy));
        if (!string.IsNullOrWhiteSpace(estado))
            lista = lista.Where(m => m.Estado.Equals(estado.Trim(), StringComparison.OrdinalIgnoreCase));
        return lista.ToList();
    }

    public async Task<MembresiaDto> ObtenerAsync(UsuarioActual actual, int id, CancellationToken ct = default)
    {
        var m = await _membresias.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("La membresía no existe.");
        VerificarAcceso(actual, m);
        return m.ToDto(_fecha.Hoy);
    }

    /// <summary>Membresía vigente hoy del usuario, o null si no tiene.</summary>
    public async Task<MembresiaDto?> ObtenerVigenteAsync(int idUsuario, CancellationToken ct = default)
    {
        var hoy = _fecha.Hoy;
        var activas = await _membresias.ListarActivasDeUsuarioAsync(idUsuario, ct);
        return activas.Where(m => m.EstaVigente(hoy)).OrderByDescending(m => m.FechaFin).FirstOrDefault()?.ToDto(hoy);
    }

    public async Task<MembresiaDto> CrearAsync(UsuarioActual actual, MembresiaRequest req, CancellationToken ct = default)
    {
        if (actual.EsEntrenador) throw new ForbiddenException("Solo el administrador o el propio cliente pueden registrar membresías.");
        var idUsuario = actual.EsCliente ? actual.IdUsuario : req.IdUsuario;
        var inicio = req.FechaInicio!.Value;

        if (inicio < _fecha.Hoy) throw new BusinessRuleException("La fecha de inicio no puede ser anterior a hoy.");
        if (inicio > _fecha.Hoy.AddDays(60)) throw new BusinessRuleException("La fecha de inicio no puede ser mayor a 60 días desde hoy.");

        await ValidarClienteAsync(idUsuario, ct);
        var plan = await ObtenerPlanActivoAsync(req.IdPlan, ct);
        var fin = Membresia.CalcularFechaFin(inicio, plan.DuracionDias);
        await ValidarSinCruceAsync(idUsuario, inicio, fin, null, ct);

        var membresia = new Membresia
        {
            IdUsuario = idUsuario,
            IdPlan = plan.IdPlan,
            Plan = plan,
            FechaInicio = inicio,
            FechaFin = fin,
            PrecioPagado = plan.Precio,
            Estado = EstadoMembresia.Activa,
            FechaRegistro = _fecha.Ahora
        };
        await _membresias.AgregarAsync(membresia, ct);
        await _uow.GuardarCambiosAsync(ct);

        return (await _membresias.ObtenerPorIdAsync(membresia.IdMembresia, ct) ?? membresia).ToDto(_fecha.Hoy);
    }

    /// <summary>Solo administrador: cambia plan y/o fecha de inicio (recalcula fin y precio).</summary>
    public async Task<MembresiaDto> ActualizarAsync(int id, MembresiaRequest req, CancellationToken ct = default)
    {
        var m = await _membresias.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("La membresía no existe.");
        if (m.Estado == EstadoMembresia.Cancelada) throw new BusinessRuleException("No se puede editar una membresía cancelada.");

        var inicio = req.FechaInicio!.Value;
        var plan = await ObtenerPlanActivoAsync(req.IdPlan, ct);
        var fin = Membresia.CalcularFechaFin(inicio, plan.DuracionDias);
        await ValidarSinCruceAsync(m.IdUsuario, inicio, fin, m.IdMembresia, ct);

        m.IdPlan = plan.IdPlan;
        m.Plan = plan;
        m.FechaInicio = inicio;
        m.FechaFin = fin;
        m.PrecioPagado = plan.Precio;
        await _uow.GuardarCambiosAsync(ct);
        return m.ToDto(_fecha.Hoy);
    }

    /// <summary>RF-08: la membresía no se borra, se cancela (queda en el historial).</summary>
    public async Task CancelarAsync(int id, CancellationToken ct = default)
    {
        var m = await _membresias.ObtenerPorIdAsync(id, ct) ?? throw new NotFoundException("La membresía no existe.");
        if (m.Estado == EstadoMembresia.Cancelada) throw new BusinessRuleException("La membresía ya está cancelada.");
        m.Estado = EstadoMembresia.Cancelada;
        await _uow.GuardarCambiosAsync(ct);
    }

    private static void VerificarAcceso(UsuarioActual actual, Membresia m)
    {
        if (actual.EsCliente && m.IdUsuario != actual.IdUsuario)
            throw new ForbiddenException("No tienes permiso para ver esta membresía.");
    }

    private async Task ValidarClienteAsync(int idUsuario, CancellationToken ct)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(idUsuario, ct) ?? throw new NotFoundException("El cliente no existe.");
        if (usuario.Rol?.Nombre != RolNombres.Cliente) throw new BusinessRuleException("Las membresías solo se asignan a usuarios con rol Cliente.");
        if (!usuario.Activo) throw new BusinessRuleException("El cliente está inactivo.");
    }

    private async Task<Plan> ObtenerPlanActivoAsync(int idPlan, CancellationToken ct)
    {
        var plan = await _planes.ObtenerPorIdAsync(idPlan, ct) ?? throw new NotFoundException("El plan no existe.");
        if (!plan.Activo) throw new BusinessRuleException("El plan seleccionado no está disponible.");
        return plan;
    }

    private async Task ValidarSinCruceAsync(int idUsuario, DateOnly inicio, DateOnly fin, int? excluirId, CancellationToken ct)
    {
        var activas = await _membresias.ListarActivasDeUsuarioAsync(idUsuario, ct);
        var cruce = activas.FirstOrDefault(m => m.IdMembresia != excluirId && m.SeCruzaCon(inicio, fin));
        if (cruce is not null)
            throw new BusinessRuleException(
                $"El cliente ya tiene una membresía activa del {cruce.FechaInicio:dd/MM/yyyy} al {cruce.FechaFin:dd/MM/yyyy}. Elige una fecha de inicio posterior.");
    }
}
