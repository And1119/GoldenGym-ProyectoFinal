package com.goldengym.app.activities

import android.content.Intent
import android.view.View
import androidx.appcompat.app.AppCompatActivity
import androidx.lifecycle.lifecycleScope
import com.goldengym.app.R
import com.goldengym.app.preferences.SessionManager
import com.goldengym.app.repository.AuthRepository
import com.goldengym.app.repository.GymRepository
import com.goldengym.app.repository.Resultado
import com.goldengym.app.utils.visible
import com.google.android.material.appbar.MaterialToolbar
import com.google.android.material.dialog.MaterialAlertDialogBuilder
import com.google.android.material.snackbar.Snackbar
import kotlinx.coroutines.launch

/**
 * Comportamiento común de todas las pantallas:
 * - acceso a la sesión y a los repositorios,
 * - manejo uniforme de errores (401 -> Login, 403 -> diálogo de acceso denegado, resto -> Snackbar),
 * - ejecución de llamadas a la API con indicador de progreso.
 */
abstract class BaseActivity : AppCompatActivity() {

    protected val session: SessionManager by lazy { SessionManager.get(this) }
    protected val repo: GymRepository by lazy { GymRepository(this) }
    protected val authRepo: AuthRepository by lazy { AuthRepository(this) }

    protected fun configurarToolbar(toolbar: MaterialToolbar, titulo: String, conAtras: Boolean = true) {
        setSupportActionBar(toolbar)
        supportActionBar?.title = titulo
        supportActionBar?.setDisplayHomeAsUpEnabled(conAtras)
    }

    override fun onSupportNavigateUp(): Boolean {
        finish()
        return true
    }

    /** Pantallas protegidas: si el token no existe o expiró, regresa al Login. */
    protected fun verificarSesion(): Boolean {
        if (!session.haySesionValida()) {
            irALogin("Tu sesión expiró. Inicia sesión nuevamente.")
            return false
        }
        return true
    }

    /** Borra la sesión local y abre el Login limpiando la pila de pantallas. */
    fun irALogin(mensaje: String? = null) {
        session.cerrar()
        val intent = Intent(this, LoginActivity::class.java).apply {
            flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
            putExtra(LoginActivity.EXTRA_MENSAJE, mensaje)
        }
        startActivity(intent)
        finish()
    }

    fun manejarError(error: Resultado.Error) {
        when {
            error.sesionExpirada -> irALogin(error.mensaje)
            error.accesoDenegado -> MaterialAlertDialogBuilder(this)
                .setTitle(R.string.acceso_denegado)
                .setMessage(error.mensaje)
                .setPositiveButton(R.string.aceptar, null)
                .show()
            else -> mostrarMensaje(error.mensaje)
        }
    }

    fun mostrarMensaje(mensaje: String) {
        Snackbar.make(findViewById(android.R.id.content), mensaje, Snackbar.LENGTH_LONG).show()
    }

    /**
     * Ejecuta una operación del repositorio en una corrutina ligada al ciclo de vida.
     * Muestra [progreso] mientras espera y llama a [alExito] si la API respondió bien.
     */
    protected fun <T> lanzar(
        progreso: View?,
        llamada: suspend () -> Resultado<T>,
        alError: ((Resultado.Error) -> Unit)? = null,
        alExito: (T) -> Unit
    ) {
        lifecycleScope.launch {
            progreso?.visible(true)
            val resultado = llamada()
            progreso?.visible(false)
            when (resultado) {
                is Resultado.Exito -> alExito(resultado.datos)
                is Resultado.Error -> if (alError != null) alError(resultado) else manejarError(resultado)
            }
        }
    }

    protected fun confirmar(titulo: String, mensaje: String, textoAccion: String, accion: () -> Unit) {
        MaterialAlertDialogBuilder(this)
            .setTitle(titulo)
            .setMessage(mensaje)
            .setNegativeButton(R.string.cancelar, null)
            .setPositiveButton(textoAccion) { _, _ -> accion() }
            .show()
    }
}
