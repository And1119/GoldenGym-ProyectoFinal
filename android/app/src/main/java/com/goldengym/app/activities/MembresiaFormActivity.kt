package com.goldengym.app.activities

import android.os.Bundle
import android.view.Menu
import android.view.MenuItem
import android.view.View
import android.widget.AdapterView
import androidx.core.content.ContextCompat
import com.goldengym.app.R
import com.goldengym.app.databinding.ActivityMembresiaFormBinding
import com.goldengym.app.models.Membresia
import com.goldengym.app.models.MembresiaRequest
import com.goldengym.app.models.Plan
import com.goldengym.app.models.Usuario
import com.goldengym.app.repository.Resultado
import com.goldengym.app.utils.*
import java.time.LocalDate

/**
 * Membresías:
 * - Administrador: asigna un plan a un cliente, edita (plan / fecha) y cancela.
 * - Cliente: "Adquirir plan" (compra simulada) y detalle de sus membresías.
 * La API valida que no se crucen fechas con otra membresía activa (regla de negocio).
 */
class MembresiaFormActivity : BaseActivity() {

    private lateinit var binding: ActivityMembresiaFormBinding
    private var idMembresia = 0
    private var planes: List<Plan> = emptyList()
    private var clientes: List<Usuario> = emptyList()
    private var actual: Membresia? = null
    private val esEdicion get() = idMembresia > 0
    /** Solo el administrador edita; el cliente puede crear (comprar) pero no editar. */
    private val puedeGuardar get() = if (esEdicion) session.esAdmin && actual?.estado != "Cancelada" else !session.esEntrenador

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityMembresiaFormBinding.inflate(layoutInflater)
        setContentView(binding.root)
        if (!verificarSesion()) return

        idMembresia = intent.getIntExtra(EXTRA_ID, 0)
        val titulo = when {
            esEdicion -> "Detalle de membresía"
            session.esCliente -> "Adquirir plan"
            session.esEntrenador -> "Detalle del plan"
            else -> "Nueva membresía"
        }
        configurarToolbar(binding.toolbar, titulo)

        // El cliente solo compra para sí mismo: no elige cliente.
        val eligeCliente = session.esAdmin && !esEdicion
        binding.lblCliente.visible(eligeCliente)
        binding.spCliente.visible(eligeCliente)

        binding.etFechaInicio.fijarFecha(LocalDate.now())
        binding.etFechaInicio.comoSelectorDeFecha(this) { actualizarResumen() }
        binding.spPlan.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
            override fun onItemSelected(p: AdapterView<*>?, v: View?, pos: Int, id: Long) = actualizarResumen()
            override fun onNothingSelected(p: AdapterView<*>?) = Unit
        }
        binding.btnGuardar.setOnClickListener { guardar() }
        binding.btnGuardar.text = if (session.esCliente && !esEdicion) "Confirmar compra" else getString(R.string.guardar)

        cargarDatos(eligeCliente)
    }

    private fun cargarDatos(eligeCliente: Boolean) {
        lanzar(binding.progreso, { repo.planes() }) { lista ->
            planes = lista
            binding.spPlan.llenar(this, lista.map { "${it.nombre} · ${Formatos.dinero(it.precio)}" })
            val idPlan = intent.getIntExtra(EXTRA_ID_PLAN, 0)
            if (idPlan > 0) seleccionarPlan(idPlan)
            if (esEdicion) cargarMembresia()
            aplicarPermisos()
            actualizarResumen()
        }
        if (eligeCliente) {
            lanzar(null, { repo.clientesActivos() }) { lista ->
                clientes = lista
                binding.spCliente.llenar(this, lista.map { "${it.nombreCompleto} (${it.correo})" })
            }
        }
    }

    private fun cargarMembresia() {
        lanzar(binding.progreso, { repo.membresia(idMembresia) }) { m ->
            actual = m
            seleccionarPlan(m.idPlan)
            binding.etFechaInicio.fechaIso = m.fechaInicio
            binding.tvEstado.visible(true)
            binding.tvEstado.text = "${m.estado} · ${m.nombreCliente}"
            binding.tvEstado.backgroundTintList = ContextCompat.getColorStateList(this, when (m.estado) {
                "Vigente" -> R.color.verde
                "Pendiente" -> R.color.azul
                "Vencida" -> R.color.naranja
                else -> R.color.gris
            })
            aplicarPermisos()
            invalidateOptionsMenu()
            actualizarResumen()
        }
    }

    private fun seleccionarPlan(idPlan: Int) {
        val pos = planes.indexOfFirst { it.idPlan == idPlan }
        if (pos >= 0) binding.spPlan.setSelection(pos)
    }

    private fun aplicarPermisos() {
        val editable = puedeGuardar
        binding.spPlan.isEnabled = editable
        binding.tilFechaInicio.isEnabled = editable
        binding.btnGuardar.visible(editable)
    }

    /** Muestra fecha de fin y total, calculados igual que en la API (inicio + duración - 1). */
    private fun actualizarResumen() {
        val plan = planes.getOrNull(binding.spPlan.selectedItemPosition) ?: return
        binding.tvPlanDetalle.text = plan.descripcion.orEmpty()
        val inicio = binding.etFechaInicio.fechaIso?.let { runCatching { LocalDate.parse(it) }.getOrNull() } ?: return
        val fin = inicio.plusDays(plan.duracionDias.toLong() - 1)
        val pagado = actual?.precioPagado ?: plan.precio
        binding.tvResumen.text = "Plan: ${plan.nombre} (${plan.duracionDias} día(s))\n" +
                "Desde: ${Formatos.fecha(inicio.toString())}\nHasta: ${Formatos.fecha(fin.toString())}\n" +
                "Total: ${Formatos.dinero(pagado)}"
    }

    private fun guardar() {
        val plan = planes.getOrNull(binding.spPlan.selectedItemPosition)
        if (plan == null) {
            mostrarMensaje("Selecciona un plan.")
            return
        }
        val fecha = binding.etFechaInicio.fechaIso
        // Al crear no se permite una fecha pasada (la API también lo valida).
        val errorFecha = if (esEdicion) Validador.requerido(fecha, "La fecha de inicio") else Validador.fechaNoPasada(fecha, "La fecha de inicio")
        if (!binding.tilFechaInicio.validar(errorFecha)) return

        val idCliente = when {
            esEdicion -> actual?.idUsuario ?: 0
            session.esAdmin -> clientes.getOrNull(binding.spCliente.selectedItemPosition)?.idUsuario ?: 0
            else -> session.idUsuario
        }
        if (idCliente == 0) {
            mostrarMensaje("Selecciona un cliente.")
            return
        }
        val req = MembresiaRequest(idCliente, plan.idPlan, fecha!!)
        val llamada: suspend () -> Resultado<Membresia> = {
            if (esEdicion) repo.actualizarMembresia(idMembresia, req) else repo.crearMembresia(req)
        }
        lanzar(binding.progreso, llamada) { m ->
            mostrarMensaje(if (esEdicion) getString(R.string.guardado_ok) else "Membresía registrada: ${m.nombrePlan} (${m.estado}).")
            finish()
        }
    }

    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        if (esEdicion && session.esAdmin && actual?.estado != "Cancelada") {
            menuInflater.inflate(R.menu.menu_formulario, menu)
            menu.findItem(R.id.accion_eliminar)?.title = "Cancelar membresía"
        }
        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        if (item.itemId == R.id.accion_eliminar) {
            confirmar("Cancelar membresía", "La membresía quedará cancelada y el cliente no podrá registrar asistencia con ella.", "Cancelar membresía") {
                lanzar(binding.progreso, { repo.cancelarMembresia(idMembresia) }) {
                    mostrarMensaje("Membresía cancelada.")
                    finish()
                }
            }
            return true
        }
        return super.onOptionsItemSelected(item)
    }

    companion object {
        const val EXTRA_ID = "id_membresia"
        const val EXTRA_ID_PLAN = "id_plan"
    }
}
