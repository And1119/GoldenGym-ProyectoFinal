package com.goldengym.app.activities

import android.content.Intent
import android.os.Bundle
import android.view.inputmethod.EditorInfo
import com.goldengym.app.R
import com.goldengym.app.databinding.ActivityLoginBinding
import com.goldengym.app.databinding.DialogoServidorBinding
import com.goldengym.app.preferences.PreferenciasUI
import com.goldengym.app.utils.Validador
import com.goldengym.app.utils.texto
import com.goldengym.app.utils.validar
import com.google.android.material.dialog.MaterialAlertDialogBuilder

/** RF-02: inicio de sesión contra la API (JWT). Si ya hay una sesión válida pasa directo al Home. */
class LoginActivity : BaseActivity() {

    private lateinit var binding: ActivityLoginBinding
    private val prefs by lazy { PreferenciasUI.get(this) }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        if (session.haySesionValida()) {
            irAHome()
            return
        }

        binding = ActivityLoginBinding.inflate(layoutInflater)
        setContentView(binding.root)

        // RF-11: preferencia "recordar correo".
        binding.cbRecordar.isChecked = prefs.recordarCorreo
        if (prefs.recordarCorreo) binding.etCorreo.setText(prefs.correoRecordado)

        intent.getStringExtra(EXTRA_MENSAJE)?.let { mostrarMensaje(it) }

        binding.btnIngresar.setOnClickListener { iniciarSesion() }
        binding.etPassword.setOnEditorActionListener { _, accion, _ ->
            if (accion == EditorInfo.IME_ACTION_DONE) { iniciarSesion(); true } else false
        }
        binding.btnIrRegistro.setOnClickListener { startActivity(Intent(this, RegistroActivity::class.java)) }
        binding.btnConfig.setOnClickListener { mostrarConfiguracionServidor() }
    }

    private fun iniciarSesion() {
        val correo = binding.etCorreo.texto
        val password = binding.etPassword.text?.toString().orEmpty()

        val okCorreo = binding.tilCorreo.validar(Validador.correo(correo))
        val okPassword = binding.tilPassword.validar(Validador.requerido(password, "La contraseña"))
        if (!okCorreo || !okPassword) return

        binding.btnIngresar.isEnabled = false
        lanzar(
            progreso = binding.progreso,
            llamada = { authRepo.login(correo, password) },
            // En el login un 401 significa credenciales incorrectas, no sesión expirada.
            alError = { error ->
                binding.btnIngresar.isEnabled = true
                mostrarMensaje(error.mensaje)
            }
        ) {
            prefs.recordarCorreo = binding.cbRecordar.isChecked
            prefs.correoRecordado = if (binding.cbRecordar.isChecked) correo else ""
            irAHome()
        }
    }

    private fun irAHome() {
        startActivity(Intent(this, HomeActivity::class.java))
        finish()
    }

    /** Permite cambiar la URL de la API (emulador o celular físico) y probar la conexión. */
    private fun mostrarConfiguracionServidor() {
        val dialogo = DialogoServidorBinding.inflate(layoutInflater)
        dialogo.etUrl.setText(prefs.urlServidor)

        MaterialAlertDialogBuilder(this)
            .setTitle(R.string.config_servidor)
            .setView(dialogo.root)
            .setNegativeButton(R.string.cancelar, null)
            .setNeutralButton(R.string.probar_conexion) { _, _ ->
                guardarUrl(dialogo.etUrl.texto)
                lanzar(binding.progreso, { authRepo.probarServidor() }) {
                    mostrarMensaje("Conexión exitosa con ${prefs.urlServidor}")
                }
            }
            .setPositiveButton(R.string.guardar) { _, _ -> guardarUrl(dialogo.etUrl.texto) }
            .show()
    }

    private fun guardarUrl(url: String) {
        if (url.startsWith("http://") || url.startsWith("https://")) {
            prefs.urlServidor = url
        } else {
            mostrarMensaje("La URL debe iniciar con http:// o https://")
        }
    }

    companion object {
        const val EXTRA_MENSAJE = "mensaje"
    }
}
