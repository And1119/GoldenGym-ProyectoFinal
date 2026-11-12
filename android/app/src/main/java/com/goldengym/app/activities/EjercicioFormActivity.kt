package com.goldengym.app.activities

import android.os.Bundle
import android.view.Menu
import android.view.MenuItem
import com.goldengym.app.R
import com.goldengym.app.databinding.ActivityEjercicioFormBinding
import com.goldengym.app.models.Ejercicio
import com.goldengym.app.models.EjercicioRequest
import com.goldengym.app.models.GruposMusculares
import com.goldengym.app.repository.Resultado
import com.goldengym.app.utils.*

/** Administrador / Entrenador: crear, editar y desactivar ejercicios. */
class EjercicioFormActivity : BaseActivity() {

    private lateinit var binding: ActivityEjercicioFormBinding
    private var idEjercicio = 0
    private val esEdicion get() = idEjercicio > 0

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityEjercicioFormBinding.inflate(layoutInflater)
        setContentView(binding.root)
        if (!verificarSesion()) return

        idEjercicio = intent.getIntExtra(EXTRA_ID, 0)
        configurarToolbar(binding.toolbar, if (esEdicion) "Editar ejercicio" else "Nuevo ejercicio")
        binding.spGrupo.llenar(this, GruposMusculares.TODOS)
        binding.btnGuardar.setOnClickListener { guardar() }
        if (esEdicion) lanzar(binding.progreso, { repo.ejercicio(idEjercicio) }) { mostrar(it) }
    }

    private fun mostrar(e: Ejercicio) = with(binding) {
        etNombre.setText(e.nombre)
        spGrupo.seleccionar(e.grupoMuscular)
        etDescripcion.setText(e.descripcion.orEmpty())
        swActivo.isChecked = e.activo
    }

    private fun guardar() {
        if (!binding.tilNombre.validar(Validador.nombre(binding.etNombre.texto, "El nombre del ejercicio"))) return
        val req = with(binding) {
            EjercicioRequest(etNombre.texto, spGrupo.selectedItem.toString(), etDescripcion.textoONull, swActivo.isChecked)
        }
        val llamada: suspend () -> Resultado<Ejercicio> = { if (esEdicion) repo.actualizarEjercicio(idEjercicio, req) else repo.crearEjercicio(req) }
        lanzar(binding.progreso, llamada, alError = { error ->
            error.errorDe("nombre")?.let { binding.tilNombre.error = it }
            if (error.codigo == 409) binding.tilNombre.error = error.mensaje
            manejarError(error)
        }) {
            mostrarMensaje(getString(R.string.guardado_ok))
            finish()
        }
    }

    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        if (esEdicion) menuInflater.inflate(R.menu.menu_formulario, menu)
        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        if (item.itemId == R.id.accion_eliminar) {
            confirmar(getString(R.string.desactivar), "El ejercicio no se podrá asignar a nuevas rutinas.", getString(R.string.desactivar)) {
                lanzar(binding.progreso, { repo.desactivarEjercicio(idEjercicio) }) { finish() }
            }
            return true
        }
        return super.onOptionsItemSelected(item)
    }

    companion object {
        const val EXTRA_ID = "id_ejercicio"
    }
}
