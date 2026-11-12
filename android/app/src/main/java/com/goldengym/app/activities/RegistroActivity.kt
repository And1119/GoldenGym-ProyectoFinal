package com.goldengym.app.activities

import android.content.Intent
import android.os.Bundle
import com.goldengym.app.R
import com.goldengym.app.databinding.ActivityRegistroBinding
import com.goldengym.app.models.RegistroRequest
import com.goldengym.app.repository.Resultado
import com.goldengym.app.utils.*

/** RF-01: registro de clientes con validación en el cliente y correo único validado por la API. */
class RegistroActivity : BaseActivity() {

    private lateinit var binding: ActivityRegistroBinding

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityRegistroBinding.inflate(layoutInflater)
        setContentView(binding.root)
        configurarToolbar(binding.toolbar, getString(R.string.registro_titulo))

        binding.etFechaNacimiento.comoSelectorDeFecha(this)
        binding.btnRegistrar.setOnClickListener { registrar() }
    }

    private fun validarFormulario(): Boolean = with(binding) {
        val password = etPassword.text?.toString().orEmpty()
        listOf(
            tilNombres.validar(Validador.nombre(etNombres.texto, "El nombre")),
            tilApellidos.validar(Validador.nombre(etApellidos.texto, "El apellido")),
            tilCorreo.validar(Validador.correo(etCorreo.texto)),
            tilTelefono.validar(Validador.telefono(etTelefono.texto)),
            tilDocumento.validar(Validador.documento(etDocumento.texto)),
            tilFechaNacimiento.validar(Validador.fechaNoFutura(etFechaNacimiento.fechaIso, "La fecha de nacimiento")),
            tilPassword.validar(Validador.password(password)),
            tilConfirmar.validar(Validador.confirmacion(password, etConfirmar.text?.toString()))
        ).all { it }
    }

    private fun registrar() {
        if (!validarFormulario()) return
        val req = with(binding) {
            RegistroRequest(
                nombres = etNombres.texto,
                apellidos = etApellidos.texto,
                correo = etCorreo.texto,
                password = etPassword.text?.toString().orEmpty(),
                telefono = etTelefono.textoONull,
                documento = etDocumento.textoONull,
                fechaNacimiento = etFechaNacimiento.fechaIso
            )
        }
        binding.btnRegistrar.isEnabled = false
        lanzar(binding.progreso, { authRepo.registrar(req) }, alError = { mostrarErroresServidor(it) }) {
            startActivity(Intent(this, HomeActivity::class.java).apply {
                flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
            })
            finish()
        }
    }

    /** Muestra bajo cada campo los errores de validación que devolvió el servidor (400) o el 409 de correo duplicado. */
    private fun mostrarErroresServidor(error: Resultado.Error) {
        binding.btnRegistrar.isEnabled = true
        with(binding) {
            error.errorDe("nombres")?.let { tilNombres.error = it }
            error.errorDe("apellidos")?.let { tilApellidos.error = it }
            error.errorDe("correo")?.let { tilCorreo.error = it }
            error.errorDe("password")?.let { tilPassword.error = it }
            error.errorDe("telefono")?.let { tilTelefono.error = it }
            error.errorDe("documento")?.let { tilDocumento.error = it }
            if (error.codigo == 409 && error.mensaje.contains("correo", ignoreCase = true)) tilCorreo.error = error.mensaje
        }
        mostrarMensaje(error.mensaje)
    }
}
