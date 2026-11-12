package com.goldengym.app.activities

import android.content.DialogInterface
import android.os.Bundle
import com.goldengym.app.R
import com.goldengym.app.databinding.ActivityPerfilBinding
import com.goldengym.app.databinding.DialogoPasswordBinding
import com.goldengym.app.models.PerfilUpdateRequest
import com.goldengym.app.models.Usuario
import com.goldengym.app.preferences.PreferenciasUI
import com.goldengym.app.utils.*
import com.google.android.material.dialog.MaterialAlertDialogBuilder

/**
 * Perfil del usuario autenticado: editar datos personales, cambiar contraseña,
 * preferencias de interfaz (RF-11) y cierre de sesión (RF-12).
 */
class PerfilActivity : BaseActivity() {

    private lateinit var binding: ActivityPerfilBinding
    private val prefs by lazy { PreferenciasUI.get(this) }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityPerfilBinding.inflate(layoutInflater)
        setContentView(binding.root)
        if (!verificarSesion()) return

        configurarToolbar(binding.toolbar, getString(R.string.perfil))
        binding.etFechaNacimiento.comoSelectorDeFecha(this)

        // Preferencias guardadas localmente (se conservan entre sesiones).
        binding.swModoOscuro.isChecked = prefs.modoOscuro
        binding.swRecordarCorreo.isChecked = prefs.recordarCorreo
        binding.swModoOscuro.setOnCheckedChangeListener { _, activo ->
            prefs.modoOscuro = activo
            prefs.aplicarTema() // recrea la pantalla con el nuevo tema
        }
        binding.swRecordarCorreo.setOnCheckedChangeListener { _, activo ->
            prefs.recordarCorreo = activo
            if (!activo) prefs.correoRecordado = ""
        }

        binding.btnGuardar.setOnClickListener { guardar() }
        binding.btnCambiarPassword.setOnClickListener { dialogoPassword() }
        binding.btnCerrarSesion.setOnClickListener {
            confirmar(getString(R.string.cerrar_sesion), getString(R.string.confirmar_salir), getString(R.string.cerrar_sesion)) {
                authRepo.cerrarSesion()
                irALogin("Sesión cerrada correctamente.")
            }
        }

        lanzar(binding.progreso, { authRepo.perfil() }) { mostrar(it) }
    }

    private fun mostrar(u: Usuario) = with(binding) {
        tvCorreo.text = u.correo
        tvRol.text = "Rol: ${u.rol}" + (u.documento?.let { " · Documento: $it" } ?: "")
        etNombres.setText(u.nombres)
        etApellidos.setText(u.apellidos)
        etTelefono.setText(u.telefono.orEmpty())
        etFechaNacimiento.fechaIso = u.fechaNacimiento
    }

    private fun guardar() {
        val ok = with(binding) {
            listOf(
                tilNombres.validar(Validador.nombre(etNombres.texto, "El nombre")),
                tilApellidos.validar(Validador.nombre(etApellidos.texto, "El apellido")),
                tilTelefono.validar(Validador.telefono(etTelefono.texto)),
                tilFechaNacimiento.validar(Validador.fechaNoFutura(etFechaNacimiento.fechaIso, "La fecha de nacimiento"))
            ).all { it }
        }
        if (!ok) return
        val req = with(binding) {
            PerfilUpdateRequest(etNombres.texto, etApellidos.texto, etTelefono.textoONull, etFechaNacimiento.fechaIso)
        }
        lanzar(binding.progreso, { authRepo.actualizarPerfil(req) }) {
            mostrarMensaje(getString(R.string.guardado_ok))
        }
    }

    private fun dialogoPassword() {
        val d = DialogoPasswordBinding.inflate(layoutInflater)
        val dialogo = MaterialAlertDialogBuilder(this)
            .setTitle(R.string.cambiar_password)
            .setView(d.root)
            .setNegativeButton(R.string.cancelar, null)
            .setPositiveButton(R.string.guardar, null)
            .create()
        dialogo.setOnShowListener {
            dialogo.getButton(DialogInterface.BUTTON_POSITIVE).setOnClickListener {
                val actual = d.etActual.text?.toString().orEmpty()
                val nueva = d.etNueva.text?.toString().orEmpty()
                val ok = d.tilActual.validar(Validador.requerido(actual, "La contraseña actual")) and
                        d.tilNueva.validar(Validador.password(nueva))
                if (!ok) return@setOnClickListener
                lanzar(binding.progreso, { authRepo.cambiarPassword(actual, nueva) }, alError = { error ->
                    if (error.codigo == 422) d.tilActual.error = error.mensaje else manejarError(error)
                }) {
                    dialogo.dismiss()
                    mostrarMensaje("Contraseña actualizada.")
                }
            }
        }
        dialogo.show()
    }
}
