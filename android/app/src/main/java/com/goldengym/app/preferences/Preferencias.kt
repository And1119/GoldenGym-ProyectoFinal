package com.goldengym.app.preferences

import android.content.Context
import android.content.SharedPreferences
import androidx.appcompat.app.AppCompatDelegate
import androidx.core.content.edit
import androidx.security.crypto.EncryptedSharedPreferences
import androidx.security.crypto.MasterKey
import com.goldengym.app.BuildConfig
import com.goldengym.app.models.AuthResponse
import com.goldengym.app.models.Roles
import java.time.Instant

/**
 * Sesión del usuario autenticado. El token JWT se guarda CIFRADO con
 * EncryptedSharedPreferences (AES-256, llave en el Android Keystore).
 * RF-12: cerrar() borra todas las credenciales locales.
 */
class SessionManager private constructor(context: Context) {

    private val prefs: SharedPreferences = crearPreferenciasCifradas(context.applicationContext)

    val token: String? get() = prefs.getString(KEY_TOKEN, null)
    val idUsuario: Int get() = prefs.getInt(KEY_ID, 0)
    val rol: String get() = prefs.getString(KEY_ROL, "") ?: ""
    val nombre: String get() = prefs.getString(KEY_NOMBRE, "") ?: ""
    val correo: String get() = prefs.getString(KEY_CORREO, "") ?: ""

    val esAdmin: Boolean get() = rol == Roles.ADMINISTRADOR
    val esEntrenador: Boolean get() = rol == Roles.ENTRENADOR
    val esCliente: Boolean get() = rol == Roles.CLIENTE
    /** Administrador o Entrenador. */
    val esPersonal: Boolean get() = esAdmin || esEntrenador

    /** Hay token y todavía no ha expirado (la API también lo valida). */
    fun haySesionValida(): Boolean {
        if (token.isNullOrBlank()) return false
        val expira = prefs.getLong(KEY_EXPIRA, 0L)
        return expira == 0L || Instant.now().toEpochMilli() < expira
    }

    fun guardar(auth: AuthResponse) {
        val expira = runCatching { Instant.parse(normalizarUtc(auth.expiraEnUtc)).toEpochMilli() }.getOrDefault(0L)
        prefs.edit {
            putString(KEY_TOKEN, auth.token)
            putLong(KEY_EXPIRA, expira)
            putInt(KEY_ID, auth.usuario.idUsuario)
            putString(KEY_ROL, auth.usuario.rol)
            putString(KEY_NOMBRE, auth.usuario.nombreCompleto)
            putString(KEY_CORREO, auth.usuario.correo)
        }
    }

    fun actualizarNombre(nombre: String) = prefs.edit { putString(KEY_NOMBRE, nombre) }

    fun cerrar() = prefs.edit { clear() }

    /** .NET puede enviar "2026-11-12T16:30:00.1234567Z": se recorta a milisegundos para Instant.parse. */
    private fun normalizarUtc(valor: String): String {
        val sinZ = valor.removeSuffix("Z")
        val partes = sinZ.split(".")
        val base = if (partes.size == 2) partes[0] + "." + partes[1].take(3).padEnd(3, '0') else sinZ
        return "${base}Z"
    }

    companion object {
        private const val ARCHIVO = "goldengym_sesion_segura"
        private const val KEY_TOKEN = "token"
        private const val KEY_EXPIRA = "expira"
        private const val KEY_ID = "id_usuario"
        private const val KEY_ROL = "rol"
        private const val KEY_NOMBRE = "nombre"
        private const val KEY_CORREO = "correo"

        @Volatile private var instancia: SessionManager? = null

        fun get(context: Context): SessionManager =
            instancia ?: synchronized(this) { instancia ?: SessionManager(context).also { instancia = it } }

        private fun crearPreferenciasCifradas(context: Context): SharedPreferences {
            fun crear(): SharedPreferences {
                val masterKey = MasterKey.Builder(context).setKeyScheme(MasterKey.KeyScheme.AES256_GCM).build()
                return EncryptedSharedPreferences.create(
                    context, ARCHIVO, masterKey,
                    EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV,
                    EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM
                )
            }
            return try {
                crear()
            } catch (e: Exception) {
                // Si la llave del Keystore se dañó (p. ej. tras restaurar el equipo), se borra la sesión y se recrea.
                context.deleteSharedPreferences(ARCHIVO)
                crear()
            }
        }
    }
}

/**
 * RF-11: preferencias de interfaz (no sensibles) que se conservan entre sesiones.
 * Modo oscuro, recordar correo y URL del servidor.
 */
class PreferenciasUI private constructor(context: Context) {

    private val prefs = context.applicationContext.getSharedPreferences("goldengym_preferencias", Context.MODE_PRIVATE)

    var modoOscuro: Boolean
        get() = prefs.getBoolean(KEY_OSCURO, false)
        set(valor) = prefs.edit { putBoolean(KEY_OSCURO, valor) }

    var recordarCorreo: Boolean
        get() = prefs.getBoolean(KEY_RECORDAR, true)
        set(valor) = prefs.edit { putBoolean(KEY_RECORDAR, valor) }

    var correoRecordado: String
        get() = prefs.getString(KEY_CORREO, "") ?: ""
        set(valor) = prefs.edit { putString(KEY_CORREO, valor) }

    var urlServidor: String
        get() = prefs.getString(KEY_URL, BuildConfig.API_BASE_URL) ?: BuildConfig.API_BASE_URL
        set(valor) = prefs.edit { putString(KEY_URL, if (valor.endsWith("/")) valor else "$valor/") }

    fun aplicarTema() {
        AppCompatDelegate.setDefaultNightMode(
            if (modoOscuro) AppCompatDelegate.MODE_NIGHT_YES else AppCompatDelegate.MODE_NIGHT_NO
        )
    }

    companion object {
        private const val KEY_OSCURO = "modo_oscuro"
        private const val KEY_RECORDAR = "recordar_correo"
        private const val KEY_CORREO = "correo_recordado"
        private const val KEY_URL = "url_servidor"

        @Volatile private var instancia: PreferenciasUI? = null

        fun get(context: Context): PreferenciasUI =
            instancia ?: synchronized(this) { instancia ?: PreferenciasUI(context).also { instancia = it } }
    }
}
