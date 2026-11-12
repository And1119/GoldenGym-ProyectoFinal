package com.goldengym.app.models

/*
 * Modelos (DTO) que viajan entre la app y la API REST en formato JSON.
 * Los nombres de las propiedades coinciden con el JSON (camelCase) que devuelve ASP.NET Core.
 * Todos los parámetros tienen valor por defecto para que Gson use el constructor vacío.
 */

// ------------------------- Autenticación -------------------------

data class LoginRequest(val correo: String = "", val password: String = "")

data class RegistroRequest(
    val nombres: String = "",
    val apellidos: String = "",
    val correo: String = "",
    val password: String = "",
    val telefono: String? = null,
    val documento: String? = null,
    val fechaNacimiento: String? = null
)

data class AuthResponse(
    val token: String = "",
    val expiraEnUtc: String = "",
    val usuario: Usuario = Usuario()
)

data class Usuario(
    val idUsuario: Int = 0,
    val nombres: String = "",
    val apellidos: String = "",
    val nombreCompleto: String = "",
    val correo: String = "",
    val telefono: String? = null,
    val documento: String? = null,
    val fechaNacimiento: String? = null,
    val rol: String = "",
    val activo: Boolean = true,
    val fechaRegistro: String? = null
)

data class PerfilUpdateRequest(
    val nombres: String = "",
    val apellidos: String = "",
    val telefono: String? = null,
    val fechaNacimiento: String? = null
)

data class CambioPasswordRequest(val passwordActual: String = "", val passwordNueva: String = "")

data class UsuarioCreateRequest(
    val nombres: String = "",
    val apellidos: String = "",
    val correo: String = "",
    val password: String = "",
    val telefono: String? = null,
    val documento: String? = null,
    val fechaNacimiento: String? = null,
    val rol: String = ""
)

data class UsuarioUpdateRequest(
    val nombres: String = "",
    val apellidos: String = "",
    val telefono: String? = null,
    val documento: String? = null,
    val fechaNacimiento: String? = null,
    val rol: String = "",
    val activo: Boolean = true
)

// ------------------------- Planes y membresías -------------------------

data class Plan(
    val idPlan: Int = 0,
    val nombre: String = "",
    val descripcion: String? = null,
    val duracionDias: Int = 0,
    val precio: Double = 0.0,
    val activo: Boolean = true
)

data class PlanRequest(
    val nombre: String = "",
    val descripcion: String? = null,
    val duracionDias: Int = 0,
    val precio: Double = 0.0,
    val activo: Boolean = true
)

data class Membresia(
    val idMembresia: Int = 0,
    val idUsuario: Int = 0,
    val nombreCliente: String = "",
    val idPlan: Int = 0,
    val nombrePlan: String = "",
    val fechaInicio: String = "",
    val fechaFin: String = "",
    val precioPagado: Double = 0.0,
    val estado: String = "",
    val diasRestantes: Int = 0
)

data class MembresiaRequest(val idUsuario: Int = 0, val idPlan: Int = 0, val fechaInicio: String = "")

// ------------------------- Ejercicios y rutinas -------------------------

data class Ejercicio(
    val idEjercicio: Int = 0,
    val nombre: String = "",
    val grupoMuscular: String = "",
    val descripcion: String? = null,
    val activo: Boolean = true
)

data class EjercicioRequest(
    val nombre: String = "",
    val grupoMuscular: String = "",
    val descripcion: String? = null,
    val activo: Boolean = true
)

data class RutinaEjercicio(
    val idEjercicio: Int = 0,
    val nombreEjercicio: String = "",
    val grupoMuscular: String = "",
    val series: Int = 0,
    val repeticiones: Int = 0,
    val descansoSeg: Int = 0,
    val orden: Int = 0
)

data class Rutina(
    val idRutina: Int = 0,
    val nombre: String = "",
    val objetivo: String? = null,
    val nivel: String = "",
    val idCliente: Int = 0,
    val nombreCliente: String = "",
    val idEntrenador: Int = 0,
    val nombreEntrenador: String = "",
    val fechaCreacion: String = "",
    val activa: Boolean = true,
    val cantidadEjercicios: Int = 0,
    val ejercicios: List<RutinaEjercicio> = emptyList()
)

data class RutinaEjercicioRequest(
    val idEjercicio: Int = 0,
    val series: Int = 0,
    val repeticiones: Int = 0,
    val descansoSeg: Int = 60,
    val orden: Int = 1
)

data class RutinaRequest(
    val nombre: String = "",
    val objetivo: String? = null,
    val nivel: String = "",
    val idCliente: Int = 0,
    val activa: Boolean = true,
    val ejercicios: List<RutinaEjercicioRequest> = emptyList()
)

// ------------------------- Asistencia y progreso -------------------------

data class Asistencia(
    val idAsistencia: Int = 0,
    val idUsuario: Int = 0,
    val nombreCliente: String = "",
    val fechaEntrada: String = "",
    val fechaSalida: String? = null,
    val minutosEnGimnasio: Int? = null,
    val registradoPor: String? = null
)

data class AsistenciaEntradaRequest(val idUsuario: Int? = null)

data class Progreso(
    val idProgreso: Int = 0,
    val idUsuario: Int = 0,
    val nombreCliente: String = "",
    val idEntrenador: Int? = null,
    val nombreEntrenador: String? = null,
    val fecha: String = "",
    val pesoKg: Double = 0.0,
    val estaturaCm: Double? = null,
    val grasaCorporal: Double? = null,
    val imc: Double? = null,
    val observaciones: String? = null
)

data class ProgresoRequest(
    val idUsuario: Int = 0,
    val fecha: String = "",
    val pesoKg: Double = 0.0,
    val estaturaCm: Double? = null,
    val grasaCorporal: Double? = null,
    val observaciones: String? = null
)

// ------------------------- Home y errores -------------------------

data class Indicador(val titulo: String = "", val valor: String = "")

data class Dashboard(
    val usuario: Usuario = Usuario(),
    val indicadores: List<Indicador> = emptyList(),
    val membresiaVigente: Membresia? = null
)

/** Formato de error que devuelve la API: { status, mensaje, errores, exito }. */
data class ErrorResponse(
    val status: Int = 0,
    val mensaje: String = "",
    val errores: Map<String, List<String>>? = null,
    val exito: Boolean = false
)

/** Constantes de roles (deben coincidir con la tabla Roles). */
object Roles {
    const val ADMINISTRADOR = "Administrador"
    const val ENTRENADOR = "Entrenador"
    const val CLIENTE = "Cliente"
    val TODOS = listOf(ADMINISTRADOR, ENTRENADOR, CLIENTE)
}

object Niveles {
    val TODOS = listOf("Principiante", "Intermedio", "Avanzado")
}

object GruposMusculares {
    val TODOS = listOf("Pecho", "Espalda", "Piernas", "Hombros", "Brazos", "Abdomen", "Cardio", "Funcional")
}
