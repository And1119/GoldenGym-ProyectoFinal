package com.goldengym.app.api

import com.goldengym.app.models.*
import okhttp3.ResponseBody
import retrofit2.Response
import retrofit2.http.*

/**
 * Contrato Retrofit de la API REST de Golden Gym (ASP.NET Core).
 * El token JWT lo agrega automáticamente AuthInterceptor en la cabecera Authorization.
 */
interface GymApi {

    // ---------------- Salud del servidor ----------------
    @GET("api/health")
    suspend fun health(): Response<ResponseBody>

    // ---------------- Autenticación ----------------
    @POST("api/auth/login")
    suspend fun login(@Body body: LoginRequest): Response<AuthResponse>

    @POST("api/auth/registro")
    suspend fun registro(@Body body: RegistroRequest): Response<AuthResponse>

    @GET("api/auth/perfil")
    suspend fun perfil(): Response<Usuario>

    @PUT("api/auth/perfil")
    suspend fun actualizarPerfil(@Body body: PerfilUpdateRequest): Response<Usuario>

    @PUT("api/auth/password")
    suspend fun cambiarPassword(@Body body: CambioPasswordRequest): Response<Unit>

    @GET("api/dashboard")
    suspend fun dashboard(): Response<Dashboard>

    // ---------------- Usuarios ----------------
    @GET("api/usuarios")
    suspend fun usuarios(
        @Query("buscar") buscar: String? = null,
        @Query("rol") rol: String? = null,
        @Query("activo") activo: Boolean? = null
    ): Response<List<Usuario>>

    @GET("api/usuarios/{id}")
    suspend fun usuario(@Path("id") id: Int): Response<Usuario>

    @POST("api/usuarios")
    suspend fun crearUsuario(@Body body: UsuarioCreateRequest): Response<Usuario>

    @PUT("api/usuarios/{id}")
    suspend fun actualizarUsuario(@Path("id") id: Int, @Body body: UsuarioUpdateRequest): Response<Usuario>

    @DELETE("api/usuarios/{id}")
    suspend fun desactivarUsuario(@Path("id") id: Int): Response<Unit>

    // ---------------- Planes ----------------
    @GET("api/planes")
    suspend fun planes(
        @Query("buscar") buscar: String? = null,
        @Query("incluirInactivos") incluirInactivos: Boolean = false
    ): Response<List<Plan>>

    @GET("api/planes/{id}")
    suspend fun plan(@Path("id") id: Int): Response<Plan>

    @POST("api/planes")
    suspend fun crearPlan(@Body body: PlanRequest): Response<Plan>

    @PUT("api/planes/{id}")
    suspend fun actualizarPlan(@Path("id") id: Int, @Body body: PlanRequest): Response<Plan>

    @DELETE("api/planes/{id}")
    suspend fun desactivarPlan(@Path("id") id: Int): Response<Unit>

    // ---------------- Membresías ----------------
    @GET("api/membresias")
    suspend fun membresias(
        @Query("idUsuario") idUsuario: Int? = null,
        @Query("idPlan") idPlan: Int? = null,
        @Query("estado") estado: String? = null,
        @Query("buscar") buscar: String? = null
    ): Response<List<Membresia>>

    @GET("api/membresias/{id}")
    suspend fun membresia(@Path("id") id: Int): Response<Membresia>

    @POST("api/membresias")
    suspend fun crearMembresia(@Body body: MembresiaRequest): Response<Membresia>

    @PUT("api/membresias/{id}")
    suspend fun actualizarMembresia(@Path("id") id: Int, @Body body: MembresiaRequest): Response<Membresia>

    @DELETE("api/membresias/{id}")
    suspend fun cancelarMembresia(@Path("id") id: Int): Response<Unit>

    // ---------------- Ejercicios ----------------
    @GET("api/ejercicios")
    suspend fun ejercicios(
        @Query("buscar") buscar: String? = null,
        @Query("grupoMuscular") grupoMuscular: String? = null,
        @Query("incluirInactivos") incluirInactivos: Boolean = false
    ): Response<List<Ejercicio>>

    @GET("api/ejercicios/{id}")
    suspend fun ejercicio(@Path("id") id: Int): Response<Ejercicio>

    @POST("api/ejercicios")
    suspend fun crearEjercicio(@Body body: EjercicioRequest): Response<Ejercicio>

    @PUT("api/ejercicios/{id}")
    suspend fun actualizarEjercicio(@Path("id") id: Int, @Body body: EjercicioRequest): Response<Ejercicio>

    @DELETE("api/ejercicios/{id}")
    suspend fun desactivarEjercicio(@Path("id") id: Int): Response<Unit>

    // ---------------- Rutinas ----------------
    @GET("api/rutinas")
    suspend fun rutinas(
        @Query("idCliente") idCliente: Int? = null,
        @Query("buscar") buscar: String? = null,
        @Query("nivel") nivel: String? = null,
        @Query("incluirInactivas") incluirInactivas: Boolean = false
    ): Response<List<Rutina>>

    @GET("api/rutinas/{id}")
    suspend fun rutina(@Path("id") id: Int): Response<Rutina>

    @POST("api/rutinas")
    suspend fun crearRutina(@Body body: RutinaRequest): Response<Rutina>

    @PUT("api/rutinas/{id}")
    suspend fun actualizarRutina(@Path("id") id: Int, @Body body: RutinaRequest): Response<Rutina>

    @DELETE("api/rutinas/{id}")
    suspend fun desactivarRutina(@Path("id") id: Int): Response<Unit>

    // ---------------- Asistencias ----------------
    @GET("api/asistencias")
    suspend fun asistencias(
        @Query("idUsuario") idUsuario: Int? = null,
        @Query("desde") desde: String? = null,
        @Query("hasta") hasta: String? = null
    ): Response<List<Asistencia>>

    @POST("api/asistencias/entrada")
    suspend fun registrarEntrada(@Body body: AsistenciaEntradaRequest): Response<Asistencia>

    @PUT("api/asistencias/{id}/salida")
    suspend fun registrarSalida(@Path("id") id: Int): Response<Asistencia>

    @DELETE("api/asistencias/{id}")
    suspend fun eliminarAsistencia(@Path("id") id: Int): Response<Unit>

    // ---------------- Progreso ----------------
    @GET("api/progresos")
    suspend fun progresos(
        @Query("idUsuario") idUsuario: Int? = null,
        @Query("desde") desde: String? = null,
        @Query("hasta") hasta: String? = null
    ): Response<List<Progreso>>

    @GET("api/progresos/{id}")
    suspend fun progreso(@Path("id") id: Int): Response<Progreso>

    @POST("api/progresos")
    suspend fun crearProgreso(@Body body: ProgresoRequest): Response<Progreso>

    @PUT("api/progresos/{id}")
    suspend fun actualizarProgreso(@Path("id") id: Int, @Body body: ProgresoRequest): Response<Progreso>

    @DELETE("api/progresos/{id}")
    suspend fun eliminarProgreso(@Path("id") id: Int): Response<Unit>
}
