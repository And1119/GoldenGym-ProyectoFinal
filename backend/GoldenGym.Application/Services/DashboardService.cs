using System.Globalization;
using GoldenGym.Application.Common;
using GoldenGym.Application.DTOs;
using GoldenGym.Application.Interfaces;
using GoldenGym.Domain.Entities;
using GoldenGym.Domain.Exceptions;

namespace GoldenGym.Application.Services;

/// <summary>Datos de la pantalla Home: perfil autenticado + indicadores según el rol (RF-04).</summary>
public class DashboardService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IMembresiaRepository _membresias;
    private readonly IRutinaRepository _rutinas;
    private readonly IAsistenciaRepository _asistencias;
    private readonly IProgresoRepository _progresos;
    private readonly IFechaProvider _fecha;

    public DashboardService(IUsuarioRepository usuarios, IMembresiaRepository membresias, IRutinaRepository rutinas,
        IAsistenciaRepository asistencias, IProgresoRepository progresos, IFechaProvider fecha)
    {
        _usuarios = usuarios;
        _membresias = membresias;
        _rutinas = rutinas;
        _asistencias = asistencias;
        _progresos = progresos;
        _fecha = fecha;
    }

    public async Task<DashboardDto> ObtenerAsync(UsuarioActual actual, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(actual.IdUsuario, ct) ?? throw new NotFoundException("El usuario no existe.");
        var hoy = _fecha.Hoy;
        var inicioHoy = hoy.ToDateTime(TimeOnly.MinValue);
        var finHoy = hoy.ToDateTime(TimeOnly.MaxValue);
        var inicioMes = new DateOnly(hoy.Year, hoy.Month, 1).ToDateTime(TimeOnly.MinValue);
        var indicadores = new List<IndicadorDto>();
        MembresiaDto? vigente = null;

        if (actual.EsCliente)
        {
            var activas = await _membresias.ListarActivasDeUsuarioAsync(actual.IdUsuario, ct);
            vigente = activas.Where(m => m.EstaVigente(hoy)).OrderByDescending(m => m.FechaFin).FirstOrDefault()?.ToDto(hoy);
            var rutinas = await _rutinas.ListarAsync(actual.IdUsuario, null, null, null, true, ct);
            var asistenciasMes = await _asistencias.ListarAsync(actual.IdUsuario, inicioMes, finHoy, ct);
            var progresos = await _progresos.ListarAsync(actual.IdUsuario, null, null, ct);
            var ultimo = progresos.OrderByDescending(p => p.Fecha).FirstOrDefault();

            indicadores.Add(new("Membresía", vigente is null ? "Sin plan vigente" : $"{vigente.NombrePlan} · {vigente.DiasRestantes} días"));
            indicadores.Add(new("Rutinas activas", rutinas.Count.ToString()));
            indicadores.Add(new("Asistencias este mes", asistenciasMes.Count.ToString()));
            indicadores.Add(new("Último peso", ultimo is null ? "—" : $"{ultimo.PesoKg.ToString("0.0", CultureInfo.InvariantCulture)} kg"));
        }
        else
        {
            var clientes = await _usuarios.ListarAsync(null, RolNombres.Cliente, true, ct);
            var membresias = await _membresias.ListarAsync(null, null, null, ct);
            var vigentes = membresias.Count(m => m.EstaVigente(hoy));
            var porVencer = membresias.Count(m => m.EstaVigente(hoy) && m.FechaFin <= hoy.AddDays(3));
            var asistenciasHoy = await _asistencias.ListarAsync(null, inicioHoy, finHoy, ct);

            indicadores.Add(new("Clientes activos", clientes.Count.ToString()));
            indicadores.Add(new("Membresías vigentes", vigentes.ToString()));
            indicadores.Add(new("Vencen en 3 días", porVencer.ToString()));
            indicadores.Add(new("Asistencias hoy", asistenciasHoy.Count.ToString()));
            if (actual.EsEntrenador)
            {
                var mias = await _rutinas.ListarAsync(null, actual.IdUsuario, null, null, true, ct);
                indicadores.Add(new("Mis rutinas activas", mias.Count.ToString()));
            }
        }

        return new DashboardDto(usuario.ToDto(), indicadores, vigente);
    }
}
