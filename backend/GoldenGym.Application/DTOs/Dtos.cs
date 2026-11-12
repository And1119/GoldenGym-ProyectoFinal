using System.ComponentModel.DataAnnotations;

namespace GoldenGym.Application.DTOs;

// =====================================================================
//  AUTENTICACIÓN Y PERFIL
// =====================================================================

public class RegistroRequest
{
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre 2 y 80 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 80 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(120)]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [RegularExpression(Validaciones.PasswordRegex, ErrorMessage = Validaciones.PasswordMensaje)]
    public string Password { get; set; } = string.Empty;

    [RegularExpression(Validaciones.TelefonoRegex, ErrorMessage = Validaciones.TelefonoMensaje)]
    public string? Telefono { get; set; }

    [RegularExpression(Validaciones.DocumentoRegex, ErrorMessage = Validaciones.DocumentoMensaje)]
    public string? Documento { get; set; }

    public DateOnly? FechaNacimiento { get; set; }
}

public class LoginRequest
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}

public class PerfilUpdateRequest
{
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre 2 y 80 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 80 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [RegularExpression(Validaciones.TelefonoRegex, ErrorMessage = Validaciones.TelefonoMensaje)]
    public string? Telefono { get; set; }

    public DateOnly? FechaNacimiento { get; set; }
}

public class CambioPasswordRequest
{
    [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
    public string PasswordActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [RegularExpression(Validaciones.PasswordRegex, ErrorMessage = Validaciones.PasswordMensaje)]
    public string PasswordNueva { get; set; } = string.Empty;
}

public record UsuarioDto(
    int IdUsuario, string Nombres, string Apellidos, string NombreCompleto, string Correo,
    string? Telefono, string? Documento, DateOnly? FechaNacimiento, string Rol, bool Activo,
    DateTime FechaRegistro);

public record AuthResponse(string Token, DateTime ExpiraEnUtc, UsuarioDto Usuario);

// =====================================================================
//  USUARIOS (gestión por el administrador)
// =====================================================================

public class UsuarioCreateRequest : RegistroRequest
{
    [Required(ErrorMessage = "El rol es obligatorio.")]
    public string Rol { get; set; } = string.Empty;
}

public class UsuarioUpdateRequest
{
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre 2 y 80 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 80 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [RegularExpression(Validaciones.TelefonoRegex, ErrorMessage = Validaciones.TelefonoMensaje)]
    public string? Telefono { get; set; }

    [RegularExpression(Validaciones.DocumentoRegex, ErrorMessage = Validaciones.DocumentoMensaje)]
    public string? Documento { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}

// =====================================================================
//  PLANES
// =====================================================================

public class PlanRequest
{
    [Required(ErrorMessage = "El nombre del plan es obligatorio.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descripcion { get; set; }

    [Range(1, 365, ErrorMessage = "La duración debe estar entre 1 y 365 días.")]
    public int DuracionDias { get; set; }

    [Range(0, 10_000_000, ErrorMessage = "El precio debe estar entre 0 y 10.000.000.")]
    public decimal Precio { get; set; }

    public bool Activo { get; set; } = true;
}

public record PlanDto(int IdPlan, string Nombre, string? Descripcion, int DuracionDias, decimal Precio, bool Activo);

// =====================================================================
//  MEMBRESÍAS
// =====================================================================

public class MembresiaRequest
{
    /// <summary>Cliente al que se le asigna la membresía. Si quien la crea es un Cliente, se ignora y se usa su propio Id.</summary>
    public int IdUsuario { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un plan.")]
    public int IdPlan { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateOnly? FechaInicio { get; set; }
}

public record MembresiaDto(
    int IdMembresia, int IdUsuario, string NombreCliente, int IdPlan, string NombrePlan,
    DateOnly FechaInicio, DateOnly FechaFin, decimal PrecioPagado, string Estado, int DiasRestantes);

// =====================================================================
//  EJERCICIOS
// =====================================================================

public class EjercicioRequest
{
    [Required(ErrorMessage = "El nombre del ejercicio es obligatorio.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 80 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El grupo muscular es obligatorio.")]
    [StringLength(40)]
    public string GrupoMuscular { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
}

public record EjercicioDto(int IdEjercicio, string Nombre, string GrupoMuscular, string? Descripcion, bool Activo);

// =====================================================================
//  RUTINAS
// =====================================================================

public class RutinaEjercicioRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Ejercicio inválido.")]
    public int IdEjercicio { get; set; }

    [Range(1, 20, ErrorMessage = "Las series deben estar entre 1 y 20.")]
    public int Series { get; set; }

    [Range(1, 100, ErrorMessage = "Las repeticiones deben estar entre 1 y 100.")]
    public int Repeticiones { get; set; }

    [Range(0, 600, ErrorMessage = "El descanso debe estar entre 0 y 600 segundos.")]
    public int DescansoSeg { get; set; } = 60;

    [Range(1, 50)]
    public int Orden { get; set; } = 1;
}

public class RutinaRequest
{
    [Required(ErrorMessage = "El nombre de la rutina es obligatorio.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 80 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Objetivo { get; set; }

    [Required(ErrorMessage = "El nivel es obligatorio.")]
    public string Nivel { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente.")]
    public int IdCliente { get; set; }

    public bool Activa { get; set; } = true;

    [MinLength(1, ErrorMessage = "La rutina debe tener al menos un ejercicio.")]
    public List<RutinaEjercicioRequest> Ejercicios { get; set; } = new();
}

public record RutinaEjercicioDto(
    int IdEjercicio, string NombreEjercicio, string GrupoMuscular, int Series, int Repeticiones, int DescansoSeg, int Orden);

public record RutinaDto(
    int IdRutina, string Nombre, string? Objetivo, string Nivel, int IdCliente, string NombreCliente,
    int IdEntrenador, string NombreEntrenador, DateTime FechaCreacion, bool Activa, int CantidadEjercicios,
    List<RutinaEjercicioDto> Ejercicios);

// =====================================================================
//  ASISTENCIAS
// =====================================================================

public class AsistenciaEntradaRequest
{
    /// <summary>Cliente que ingresa. Si quien registra es un Cliente, se usa su propio Id.</summary>
    public int? IdUsuario { get; set; }
}

public record AsistenciaDto(
    int IdAsistencia, int IdUsuario, string NombreCliente, DateTime FechaEntrada, DateTime? FechaSalida,
    int? MinutosEnGimnasio, string? RegistradoPor);

// =====================================================================
//  PROGRESO
// =====================================================================

public class ProgresoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente.")]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateOnly? Fecha { get; set; }

    [Range(typeof(decimal), "20", "400", ErrorMessage = "El peso debe estar entre 20 y 400 kg.")]
    public decimal PesoKg { get; set; }

    [Range(typeof(decimal), "50", "250", ErrorMessage = "La estatura debe estar entre 50 y 250 cm.")]
    public decimal? EstaturaCm { get; set; }

    [Range(typeof(decimal), "2", "70", ErrorMessage = "El % de grasa debe estar entre 2 y 70.")]
    public decimal? GrasaCorporal { get; set; }

    [StringLength(300)]
    public string? Observaciones { get; set; }
}

public record ProgresoDto(
    int IdProgreso, int IdUsuario, string NombreCliente, int? IdEntrenador, string? NombreEntrenador,
    DateOnly Fecha, decimal PesoKg, decimal? EstaturaCm, decimal? GrasaCorporal, decimal? Imc, string? Observaciones);

// =====================================================================
//  DASHBOARD (Home)
// =====================================================================

public record IndicadorDto(string Titulo, string Valor);

public record DashboardDto(UsuarioDto Usuario, List<IndicadorDto> Indicadores, MembresiaDto? MembresiaVigente);

// =====================================================================
//  ERRORES
// =====================================================================

/// <summary>Formato único de error que devuelve la API (lo interpreta la app Android).</summary>
public record ErrorResponse(int Status, string Mensaje, Dictionary<string, string[]>? Errores = null)
{
    public bool Exito => false;
}

public static class Validaciones
{
    public const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,64}$";
    public const string PasswordMensaje = "La contraseña debe tener mínimo 8 caracteres, una mayúscula, una minúscula y un número.";
    public const string TelefonoRegex = @"^$|^[0-9]{7,15}$";
    public const string TelefonoMensaje = "El teléfono debe tener entre 7 y 15 dígitos.";
    public const string DocumentoRegex = @"^$|^[0-9]{5,20}$";
    public const string DocumentoMensaje = "El documento debe tener entre 5 y 20 dígitos.";
}
