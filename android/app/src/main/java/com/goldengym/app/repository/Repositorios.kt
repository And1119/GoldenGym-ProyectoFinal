package com.goldengym.app.repository

import android.content.Context
import com.goldengym.app.api.ApiClient
import com.goldengym.app.api.GymApi
import com.goldengym.app.models.*
import com.goldengym.app.preferences.SessionManager
import com.google.gson.Gson
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import retrofit2.Response
import java.io.IOException
import java.net.ConnectException
import java.net.SocketTimeoutException
import java.net.UnknownHostException

/** Resultado de una operación contra la API (RNF-08: errores HTTP y de conexión controlados). */
sealed class Resultado<out T> {
    data class Exito<T>(val datos: T) : Resultado<T>()
    data class Error(
        val mensaje: String,
        val codigo: Int? = null,
        val errores: Map<String, List<String>>? = null
    ) : Resultado<Nothing>() {
        val sesionExpirada: Boolean get() = codigo == 401
        val accesoDenegado: Boolean get() = codigo == 403
        /** Primer mensaje de error de un campo concreto (para mostrarlo bajo el TextInputLayout). */
        fun errorDe(campo: String): String? = errores?.entries
            ?.firstOrNull { it.key.equals(campo, ignoreCase = true) }?.value?.firstOrNull()
    }
}

/** Lógica común: ejecuta la llamada en un hilo de E/S y traduce la respuesta HTTP a Resultado. */
abstract class BaseRepository(context: Context) {
    protected val appContext: Context = context.applicationContext
    protected val api: GymApi get() = ApiClient.get(appContext)
    private val gson = Gson()

    protected suspend fun <T> ejecutar(llamada: suspend (GymApi) -> Response<T>): Resultado<T> =
        withContext(Dispatchers.IO) {
            try {
                val resp = llamada(api)
                if (resp.isSuccessful) {
                    @Suppress("UNCHECKED_CAST")
                    Resultado.Exito(resp.body() ?: Unit as T)
                } else {
                    errorHttp(resp)
                }
            } catch (e: SocketTimeoutException) {
                Resultado.Error("El servidor tardó demasiado en responder. Intenta de nuevo.")
            } catch (e: UnknownHostException) {
                Resultado.Error(SIN_CONEXION)
            } catch (e: ConnectException) {
                Resultado.Error(SIN_CONEXION)
            } catch (e: IOException) {
                Resultado.Error("Error de red: ${e.message ?: "sin detalle"}")
            } catch (e: Exception) {
                Resultado.Error("Respuesta inesperada del servidor: ${e.message ?: e.javaClass.simpleName}")
            }
        }

    private fun <T> errorHttp(resp: Response<T>): Resultado.Error {
        val cuerpo = runCatching { resp.errorBody()?.string() }.getOrNull()
        val error = runCatching { gson.fromJson(cuerpo, ErrorResponse::class.java) }.getOrNull()
        val mensaje = error?.mensaje?.takeIf { it.isNotBlank() } ?: when (resp.code()) {
            400 -> "Los datos enviados no son válidos."
            401 -> "Tu sesión expiró. Inicia sesión nuevamente."
            403 -> "Acceso denegado: tu rol no tiene permiso para esta acción."
            404 -> "El registro no existe."
            409 -> "El registro ya existe o está relacionado con otros datos."
            422 -> "La operación no cumple las reglas del gimnasio."
            in 500..599 -> "Error interno del servidor (${resp.code()})."
            else -> "Error HTTP ${resp.code()}."
        }
        return Resultado.Error(mensaje, resp.code(), error?.errores)
    }

    companion object {
        const val SIN_CONEXION =
            "No se pudo conectar con el servidor. Verifica que la API esté encendida y la URL configurada (ícono ⚙ en el Login)."
    }
}

/** Autenticación y perfil. Guarda / borra la sesión local. */
class AuthRepository(context: Context) : BaseRepository(context) {
    private val session = SessionManager.get(context)

    suspend fun login(correo: String, password: String): Resultado<AuthResponse> =
        ejecutar { it.login(LoginRequest(correo.trim(), password)) }.also { if (it is Resultado.Exito) session.guardar(it.datos) }

    suspend fun registrar(req: RegistroRequest): Resultado<AuthResponse> =
        ejecutar { it.registro(req) }.also { if (it is Resultado.Exito) session.guardar(it.datos) }

    suspend fun perfil() = ejecutar { it.perfil() }

    suspend fun actualizarPerfil(req: PerfilUpdateRequest): Resultado<Usuario> =
        ejecutar { it.actualizarPerfil(req) }.also { if (it is Resultado.Exito) session.actualizarNombre(it.datos.nombreCompleto) }

    suspend fun cambiarPassword(actual: String, nueva: String) = ejecutar { it.cambiarPassword(CambioPasswordRequest(actual, nueva)) }

    suspend fun probarServidor() = ejecutar { it.health() }

    /** RF-12: cierra la sesión borrando el token y los datos del usuario. */
    fun cerrarSesion() = session.cerrar()
}

/** Operaciones del negocio (CRUD de todos los módulos). */
class GymRepository(context: Context) : BaseRepository(context) {

    suspend fun dashboard() = ejecutar { it.dashboard() }

    // Usuarios
    suspend fun usuarios(buscar: String? = null, rol: String? = null, activo: Boolean? = null) =
        ejecutar { it.usuarios(buscar, rol, activo) }
    suspend fun usuario(id: Int) = ejecutar { it.usuario(id) }
    suspend fun crearUsuario(req: UsuarioCreateRequest) = ejecutar { it.crearUsuario(req) }
    suspend fun actualizarUsuario(id: Int, req: UsuarioUpdateRequest) = ejecutar { it.actualizarUsuario(id, req) }
    suspend fun desactivarUsuario(id: Int) = ejecutar { it.desactivarUsuario(id) }
    suspend fun clientesActivos() = ejecutar { it.usuarios(null, Roles.CLIENTE, true) }

    // Planes
    suspend fun planes(buscar: String? = null, incluirInactivos: Boolean = false) = ejecutar { it.planes(buscar, incluirInactivos) }
    suspend fun plan(id: Int) = ejecutar { it.plan(id) }
    suspend fun crearPlan(req: PlanRequest) = ejecutar { it.crearPlan(req) }
    suspend fun actualizarPlan(id: Int, req: PlanRequest) = ejecutar { it.actualizarPlan(id, req) }
    suspend fun desactivarPlan(id: Int) = ejecutar { it.desactivarPlan(id) }

    // Membresías
    suspend fun membresias(estado: String? = null, buscar: String? = null) = ejecutar { it.membresias(null, null, estado, buscar) }
    suspend fun membresia(id: Int) = ejecutar { it.membresia(id) }
    suspend fun crearMembresia(req: MembresiaRequest) = ejecutar { it.crearMembresia(req) }
    suspend fun actualizarMembresia(id: Int, req: MembresiaRequest) = ejecutar { it.actualizarMembresia(id, req) }
    suspend fun cancelarMembresia(id: Int) = ejecutar { it.cancelarMembresia(id) }

    // Ejercicios
    suspend fun ejercicios(buscar: String? = null, grupo: String? = null, incluirInactivos: Boolean = false) =
        ejecutar { it.ejercicios(buscar, grupo, incluirInactivos) }
    suspend fun ejercicio(id: Int) = ejecutar { it.ejercicio(id) }
    suspend fun crearEjercicio(req: EjercicioRequest) = ejecutar { it.crearEjercicio(req) }
    suspend fun actualizarEjercicio(id: Int, req: EjercicioRequest) = ejecutar { it.actualizarEjercicio(id, req) }
    suspend fun desactivarEjercicio(id: Int) = ejecutar { it.desactivarEjercicio(id) }

    // Rutinas
    suspend fun rutinas(buscar: String? = null, nivel: String? = null, incluirInactivas: Boolean = false) =
        ejecutar { it.rutinas(null, buscar, nivel, incluirInactivas) }
    suspend fun rutina(id: Int) = ejecutar { it.rutina(id) }
    suspend fun crearRutina(req: RutinaRequest) = ejecutar { it.crearRutina(req) }
    suspend fun actualizarRutina(id: Int, req: RutinaRequest) = ejecutar { it.actualizarRutina(id, req) }
    suspend fun desactivarRutina(id: Int) = ejecutar { it.desactivarRutina(id) }

    // Asistencias
    suspend fun asistencias(desde: String? = null, hasta: String? = null) = ejecutar { it.asistencias(null, desde, hasta) }
    suspend fun registrarEntrada(idUsuario: Int?) = ejecutar { it.registrarEntrada(AsistenciaEntradaRequest(idUsuario)) }
    suspend fun registrarSalida(id: Int) = ejecutar { it.registrarSalida(id) }
    suspend fun eliminarAsistencia(id: Int) = ejecutar { it.eliminarAsistencia(id) }

    // Progreso
    suspend fun progresos(desde: String? = null, hasta: String? = null) = ejecutar { it.progresos(null, desde, hasta) }
    suspend fun progreso(id: Int) = ejecutar { it.progreso(id) }
    suspend fun crearProgreso(req: ProgresoRequest) = ejecutar { it.crearProgreso(req) }
    suspend fun actualizarProgreso(id: Int, req: ProgresoRequest) = ejecutar { it.actualizarProgreso(id, req) }
    suspend fun eliminarProgreso(id: Int) = ejecutar { it.eliminarProgreso(id) }
}

/** Transforma los datos de un resultado exitoso (los errores pasan igual). */
fun <T, R> Resultado<T>.map(transformar: (T) -> R): Resultado<R> = when (this) {
    is Resultado.Exito -> Resultado.Exito(transformar(datos))
    is Resultado.Error -> this
}
