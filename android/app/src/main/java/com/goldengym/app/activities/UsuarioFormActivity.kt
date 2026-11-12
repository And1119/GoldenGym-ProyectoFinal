package com.goldengym.app.activities

import android.os.Bundle
import android.view.Menu
import android.view.MenuItem
import com.goldengym.app.R
import com.goldengym.app.databinding.ActivityUsuarioFormBinding
import com.goldengym.app.models.Roles
import com.goldengym.app.models.Usuario
import com.goldengym.app.models.UsuarioCreateRequest
import com.goldengym.app.models.UsuarioUpdateRequest
import com.goldengym.app.repository.Resultado
import com.goldengym.app.utils.*

/**
 * RF-05 / RF-07 / RF-08 / RF-10: crear, editar, desactivar y ver el detalle de un usuario.
 * Solo el administrador edita; el entrenador ve el detalle del cliente en modo lectura.
 */
class UsuarioFormActivity : BaseActivity() {

    private lateinit var binding: ActivityUsuarioFormBinding
    private var idUsuario = 0
    private val esEdicion get() = idUsuario > 0

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityUsuarioFormBinding.inflate(layoutInflater)
        setContentView(binding.root)
        if (!verificarSesion()) return

        idUsuario = intent.getIntExtra(EXTRA_ID, 0)
        configurarToolbar(binding.toolbar, if (esEdicion) "Detalle de usuario" else "Nuevo usuario")

        binding.spRol.llenar(this, Roles.TODOS)
        binding.spRol.seleccionar(Roles.CLIENTE)
        binding.etFechaNacimiento.comoSelectorDeFecha(this)
        binding.btnGuardar.setOnClickListener { guardar() }

        // En edición el correo no se cambia y la contraseña la gestiona el propio usuario.
        binding.tilCorreo.isEnabled = !esEdicion
        binding.tilPassword.visible(!esEdicion)
        binding.swActivo.visible(esEdicion)

        if (!session.esAdmin) modoLectura()
        if (esEdicion) cargar()
    }

    private fun modoLectura() = with(binding) {
        listOf(tilNombres, tilApellidos, tilCorreo, tilTelefono, tilDocumento, tilFechaNacimiento).forEach { it.isEnabled = false }
        spRol.isEnabled = false
        swActivo.isEnabled = false
        btnGuardar.visible(false)
    }

    private fun cargar() {
        lanzar(binding.progreso, { repo.usuario(idUsuario) }) { mostrar(it) }
    }

    private fun mostrar(u: Usuario) = with(binding) {
        etNombres.setText(u.nombres)
        etApellidos.setText(u.apellidos)
        etCorreo.setText(u.correo)
        etTelefono.setText(u.telefono.orEmpty())
        etDocumento.setText(u.documento.orEmpty())
        etFechaNacimiento.fechaIso = u.fechaNacimiento
        spRol.seleccionar(u.rol)
        swActivo.isChecked = u.activo
    }

    private fun validar(): Boolean = with(binding) {
        listOf(
            tilNombres.validar(Validador.nombre(etNombres.texto, "El nombre")),
            tilApellidos.validar(Validador.nombre(etApellidos.texto, "El apellido")),
            tilCorreo.validar(if (esEdicion) null else Validador.correo(etCorreo.texto)),
            tilPassword.validar(if (esEdicion) null else Validador.password(etPassword.text?.toString())),
            tilTelefono.validar(Validador.telefono(etTelefono.texto)),
            tilDocumento.validar(Validador.documento(etDocumento.texto)),
            tilFechaNacimiento.validar(Validador.fechaNoFutura(etFechaNacimiento.fechaIso, "La fecha de nacimiento"))
        ).all { it }
    }

    private fun guardar() {
        if (!validar()) return
        val rol = binding.spRol.selectedItem.toString()
        val b = binding
        val actualizar = UsuarioUpdateRequest(b.etNombres.texto, b.etApellidos.texto, b.etTelefono.textoONull,
            b.etDocumento.textoONull, b.etFechaNacimiento.fechaIso, rol, b.swActivo.isChecked)
        val crear = UsuarioCreateRequest(b.etNombres.texto, b.etApellidos.texto, b.etCorreo.texto, b.etPassword.text.toString(),
            b.etTelefono.textoONull, b.etDocumento.textoONull, b.etFechaNacimiento.fechaIso, rol)
        val llamada: suspend () -> Resultado<Usuario> = {
            if (esEdicion) repo.actualizarUsuario(idUsuario, actualizar) else repo.crearUsuario(crear)
        }
        lanzar(binding.progreso, llamada) {
            mostrarMensaje(getString(R.string.guardado_ok))
            finish()
        }
    }

    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        if (esEdicion && session.esAdmin) menuInflater.inflate(R.menu.menu_formulario, menu)
        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        if (item.itemId == R.id.accion_eliminar) {
            confirmar(getString(R.string.desactivar), "El usuario no podrá iniciar sesión. Su historial se conserva.", getString(R.string.desactivar)) {
                lanzar(binding.progreso, { repo.desactivarUsuario(idUsuario) }) {
                    mostrarMensaje("Usuario desactivado.")
                    finish()
                }
            }
            return true
        }
        return super.onOptionsItemSelected(item)
    }

    companion object {
        const val EXTRA_ID = "id_usuario"
    }
}
