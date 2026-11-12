package com.goldengym.app.activities

import android.os.Bundle
import android.view.Menu
import android.view.MenuItem
import com.goldengym.app.R
import com.goldengym.app.databinding.ActivityPlanFormBinding
import com.goldengym.app.models.Plan
import com.goldengym.app.models.PlanRequest
import com.goldengym.app.repository.Resultado
import com.goldengym.app.utils.*

/** Administrador: crear, editar y desactivar planes. */
class PlanFormActivity : BaseActivity() {

    private lateinit var binding: ActivityPlanFormBinding
    private var idPlan = 0
    private val esEdicion get() = idPlan > 0

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityPlanFormBinding.inflate(layoutInflater)
        setContentView(binding.root)
        if (!verificarSesion()) return

        idPlan = intent.getIntExtra(EXTRA_ID, 0)
        configurarToolbar(binding.toolbar, if (esEdicion) "Editar plan" else "Nuevo plan")
        binding.btnGuardar.setOnClickListener { guardar() }
        if (esEdicion) lanzar(binding.progreso, { repo.plan(idPlan) }) { mostrar(it) }
    }

    private fun mostrar(p: Plan) = with(binding) {
        etNombre.setText(p.nombre)
        etDescripcion.setText(p.descripcion.orEmpty())
        etDuracion.setText(p.duracionDias.toString())
        etPrecio.setText(p.precio.toLong().toString())
        swActivo.isChecked = p.activo
    }

    private fun guardar() {
        val ok = with(binding) {
            listOf(
                tilNombre.validar(Validador.nombre(etNombre.texto, "El nombre del plan")),
                tilDuracion.validar(Validador.enteroEnRango(etDuracion.texto, "La duración", 1, 365)),
                tilPrecio.validar(Validador.decimalEnRango(etPrecio.texto, "El precio", 0.0, 10_000_000.0))
            ).all { it }
        }
        if (!ok) return
        val req = with(binding) {
            PlanRequest(etNombre.texto, etDescripcion.textoONull, etDuracion.texto.toInt(),
                etPrecio.texto.replace(',', '.').toDouble(), swActivo.isChecked)
        }
        val llamada: suspend () -> Resultado<Plan> = { if (esEdicion) repo.actualizarPlan(idPlan, req) else repo.crearPlan(req) }
        lanzar(binding.progreso, llamada) {
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
            confirmar(getString(R.string.desactivar), "El plan dejará de ofrecerse. Las membresías ya vendidas no cambian.", getString(R.string.desactivar)) {
                lanzar(binding.progreso, { repo.desactivarPlan(idPlan) }) { finish() }
            }
            return true
        }
        return super.onOptionsItemSelected(item)
    }

    companion object {
        const val EXTRA_ID = "id_plan"
    }
}
