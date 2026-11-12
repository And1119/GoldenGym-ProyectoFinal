namespace GoldenGym.Domain.Entities;

/// <summary>Nombres de rol usados en la base de datos, en los JWT y en las políticas.</summary>
public static class RolNombres
{
    public const string Administrador = "Administrador";
    public const string Entrenador = "Entrenador";
    public const string Cliente = "Cliente";

    public static readonly string[] Todos = { Administrador, Entrenador, Cliente };
}

public class Rol
{
    public int IdRol { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}

public class Usuario
{
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Documento { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public Rol? Rol { get; set; }
    public ICollection<Membresia> Membresias { get; set; } = new List<Membresia>();

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
}

public class Plan
{
    public int IdPlan { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int DuracionDias { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; } = true;
}

public static class EstadoMembresia
{
    // Estados guardados en BD
    public const string Activa = "Activa";
    public const string Cancelada = "Cancelada";
    // Estados calculados
    public const string Vigente = "Vigente";
    public const string Pendiente = "Pendiente";
    public const string Vencida = "Vencida";
}

public class Membresia
{
    public int IdMembresia { get; set; }
    public int IdUsuario { get; set; }
    public int IdPlan { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public decimal PrecioPagado { get; set; }
    public string Estado { get; set; } = EstadoMembresia.Activa;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public Usuario? Usuario { get; set; }
    public Plan? Plan { get; set; }

    /// <summary>Regla de negocio: FechaFin = FechaInicio + duración del plan - 1 día.</summary>
    public static DateOnly CalcularFechaFin(DateOnly inicio, int duracionDias) => inicio.AddDays(duracionDias - 1);

    public bool EstaVigente(DateOnly hoy) =>
        Estado == EstadoMembresia.Activa && hoy >= FechaInicio && hoy <= FechaFin;

    public string EstadoCalculado(DateOnly hoy)
    {
        if (Estado == EstadoMembresia.Cancelada) return EstadoMembresia.Cancelada;
        if (hoy < FechaInicio) return EstadoMembresia.Pendiente;
        if (hoy > FechaFin) return EstadoMembresia.Vencida;
        return EstadoMembresia.Vigente;
    }

    public bool SeCruzaCon(DateOnly inicio, DateOnly fin) => inicio <= FechaFin && fin >= FechaInicio;
}

public class Ejercicio
{
    public int IdEjercicio { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string GrupoMuscular { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
}

public static class NivelRutina
{
    public const string Principiante = "Principiante";
    public const string Intermedio = "Intermedio";
    public const string Avanzado = "Avanzado";
    public static readonly string[] Todos = { Principiante, Intermedio, Avanzado };
}

public class Rutina
{
    public int IdRutina { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Objetivo { get; set; }
    public string Nivel { get; set; } = NivelRutina.Principiante;
    public int IdCliente { get; set; }
    public int IdEntrenador { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public bool Activa { get; set; } = true;

    public Usuario? Cliente { get; set; }
    public Usuario? Entrenador { get; set; }
    public ICollection<RutinaEjercicio> Ejercicios { get; set; } = new List<RutinaEjercicio>();
}

public class RutinaEjercicio
{
    public int IdRutina { get; set; }
    public int IdEjercicio { get; set; }
    public int Series { get; set; }
    public int Repeticiones { get; set; }
    public int DescansoSeg { get; set; } = 60;
    public int Orden { get; set; } = 1;

    public Rutina? Rutina { get; set; }
    public Ejercicio? Ejercicio { get; set; }
}

public class Asistencia
{
    public int IdAsistencia { get; set; }
    public int IdUsuario { get; set; }
    public DateTime FechaEntrada { get; set; }
    public DateTime? FechaSalida { get; set; }
    public int? IdRegistradoPor { get; set; }

    public Usuario? Usuario { get; set; }
    public Usuario? RegistradoPor { get; set; }

    public int? MinutosEnGimnasio =>
        FechaSalida.HasValue ? (int)(FechaSalida.Value - FechaEntrada).TotalMinutes : null;
}

public class Progreso
{
    public int IdProgreso { get; set; }
    public int IdUsuario { get; set; }
    public int? IdEntrenador { get; set; }
    public DateOnly Fecha { get; set; }
    public decimal PesoKg { get; set; }
    public decimal? EstaturaCm { get; set; }
    public decimal? GrasaCorporal { get; set; }
    public string? Observaciones { get; set; }

    public Usuario? Usuario { get; set; }
    public Usuario? Entrenador { get; set; }

    /// <summary>Índice de masa corporal: peso / estatura(m)^2.</summary>
    public decimal? Imc =>
        EstaturaCm is > 0 ? Math.Round(PesoKg / ((EstaturaCm.Value / 100m) * (EstaturaCm.Value / 100m)), 1) : null;
}
