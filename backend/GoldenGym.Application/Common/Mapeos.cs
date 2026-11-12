using GoldenGym.Application.DTOs;
using GoldenGym.Domain.Entities;

namespace GoldenGym.Application.Common;

/// <summary>Conversión de entidades del dominio a DTOs de respuesta.</summary>
public static class Mapeos
{
    public static UsuarioDto ToDto(this Usuario u) => new(
        u.IdUsuario, u.Nombres, u.Apellidos, u.NombreCompleto, u.Correo, u.Telefono, u.Documento,
        u.FechaNacimiento, u.Rol?.Nombre ?? string.Empty, u.Activo, u.FechaRegistro);

    public static PlanDto ToDto(this Plan p) => new(p.IdPlan, p.Nombre, p.Descripcion, p.DuracionDias, p.Precio, p.Activo);

    public static MembresiaDto ToDto(this Membresia m, DateOnly hoy)
    {
        var estado = m.EstadoCalculado(hoy);
        var diasRestantes = estado switch
        {
            EstadoMembresia.Vigente => m.FechaFin.DayNumber - hoy.DayNumber + 1,
            EstadoMembresia.Pendiente => m.FechaFin.DayNumber - m.FechaInicio.DayNumber + 1,
            _ => 0
        };
        return new MembresiaDto(m.IdMembresia, m.IdUsuario, m.Usuario?.NombreCompleto ?? string.Empty,
            m.IdPlan, m.Plan?.Nombre ?? string.Empty, m.FechaInicio, m.FechaFin, m.PrecioPagado, estado, diasRestantes);
    }

    public static EjercicioDto ToDto(this Ejercicio e) => new(e.IdEjercicio, e.Nombre, e.GrupoMuscular, e.Descripcion, e.Activo);

    public static RutinaDto ToDto(this Rutina r, bool incluirEjercicios)
    {
        var ejercicios = incluirEjercicios
            ? r.Ejercicios.OrderBy(x => x.Orden).Select(x => new RutinaEjercicioDto(
                x.IdEjercicio, x.Ejercicio?.Nombre ?? string.Empty, x.Ejercicio?.GrupoMuscular ?? string.Empty,
                x.Series, x.Repeticiones, x.DescansoSeg, x.Orden)).ToList()
            : new List<RutinaEjercicioDto>();

        return new RutinaDto(r.IdRutina, r.Nombre, r.Objetivo, r.Nivel, r.IdCliente, r.Cliente?.NombreCompleto ?? string.Empty,
            r.IdEntrenador, r.Entrenador?.NombreCompleto ?? string.Empty, r.FechaCreacion, r.Activa, r.Ejercicios.Count, ejercicios);
    }

    public static AsistenciaDto ToDto(this Asistencia a) => new(
        a.IdAsistencia, a.IdUsuario, a.Usuario?.NombreCompleto ?? string.Empty, a.FechaEntrada, a.FechaSalida,
        a.MinutosEnGimnasio, a.RegistradoPor?.NombreCompleto);

    public static ProgresoDto ToDto(this Progreso p) => new(
        p.IdProgreso, p.IdUsuario, p.Usuario?.NombreCompleto ?? string.Empty, p.IdEntrenador, p.Entrenador?.NombreCompleto,
        p.Fecha, p.PesoKg, p.EstaturaCm, p.GrasaCorporal, p.Imc, p.Observaciones);
}
