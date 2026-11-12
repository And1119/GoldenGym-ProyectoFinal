package com.goldengym.app.activities

import android.os.Bundle
import android.view.Menu
import android.view.MenuItem
import com.goldengym.app.R
import com.goldengym.app.databinding.ActivityProgresoFormBinding
import com.goldengym.app.models.Progreso
import com.goldengym.app.models.ProgresoRequest
import com.goldengym.app.models.Usuario
import com.goldengym.app.repository.Resultado
import com.goldengym.app.utils.*
import java.time.LocalDate

/** Entrenador / Administrador: registrar, editar y eliminar mediciones de progreso de un cliente. */
class ProgresoFormActivity : BaseActivity() {

    private lateinit var binding: ActivityProgresoFormBinding
    private var idProgreso = 0
    private var clientes: List<Usuario> = emptyList()
    private var pendienteIdCliente: Int? = null
    private val esEdicion get() = idProgreso > 0

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityProgresoFormBinding.inflate(layoutInflater)
        setContentView(binding.root)
        if (!verificarSesion()) return

        idProgreso = intent.getIntExtra(EXTRA_ID, 0)
        configurarToolbar(binding.toolbar, if (esEdicion) "Editar medición" else "Nueva medición")
        binding.etFecha.fijarFecha(LocalDate.now())
        binding.etFecha.comoSelectorDeFecha(this)
        binding.btnGuardar.setOnClickListener { guardar() }

        lanzar(binding.progreso, { repo.clientesActivos() }) { lista ->
            clientes = lista
            binding.spCliente.llenar(this, lista.map { it.nombreCompleto })
            pendienteIdCliente?.let { seleccionarCliente(it) }
        }
        if (esEdicion) lanzar(binding.progreso, { repo.progreso(idProgreso) }) { mostrar(it) }
    }

    private fun mostrar(p: Progreso) = with(binding) {
        pendienteIdCliente = p.idUsuario
        seleccionarCliente(p.idUsuario)
        etFecha.fechaIso = p.fecha
        etPeso.setText(Formatos.numero(p.pesoKg))
        etEstatura.setText(p.estaturaCm?.let { Formatos.numero(it) }.orEmpty())
        etGrasa.setText(p.grasaCorporal?.let { Formatos.numero(it) }.orEmpty())
        etObservaciones.setText(p.observaciones.orEmpty())
    }

    private fun seleccionarCliente(id: Int) {
        val pos = clientes.indexOfFirst { it.idUsuario == id }
        if (pos >= 0) binding.spCliente.setSelection(pos)
    }

    private fun decimal(texto: String): Double? = texto.replace(',', '.').toDoubleOrNull()

    private fun guardar() {
        val ok = with(binding) {
            listOf(
                tilFecha.validar(Validador.requerido(etFecha.fechaIso, "La fecha") ?: Validador.fechaNoFutura(etFecha.fechaIso, "La fecha")),
                tilPeso.validar(Validador.decimalEnRango(etPeso.texto, "El peso", 20.0, 400.0)),
                tilEstatura.validar(Validador.decimalEnRango(etEstatura.texto, "La estatura", 50.0, 250.0, opcional = true)),
                tilGrasa.validar(Validador.decimalEnRango(etGrasa.texto, "El % de grasa", 2.0, 70.0, opcional = true))
            ).all { it }
        }
        if (!ok) return
        val cliente = clientes.getOrNull(binding.spCliente.selectedItemPosition)
        if (cliente == null) {
            mostrarMensaje("Selecciona un cliente.")
            return
        }
        val req = with(binding) {
            ProgresoRequest(cliente.idUsuario, etFecha.fechaIso!!, decimal(etPeso.texto) ?: 0.0,
                decimal(etEstatura.texto), decimal(etGrasa.texto), etObservaciones.textoONull)
        }
        val llamada: suspend () -> Resultado<Progreso> = {
            if (esEdicion) repo.actualizarProgreso(idProgreso, req) else repo.crearProgreso(req)
        }
        lanzar(binding.progreso, llamada) { p ->
            mostrarMensaje("Medición guardada. IMC: ${p.imc?.let { Formatos.numero(it) } ?: "—"}")
            finish()
        }
    }

    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        if (esEdicion) {
            menuInflater.inflate(R.menu.menu_formulario, menu)
            menu.findItem(R.id.accion_eliminar)?.title = getString(R.string.eliminar)
        }
        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        if (item.itemId == R.id.accion_eliminar) {
            confirmar(getString(R.string.eliminar), "¿Eliminar esta medición?", getString(R.string.eliminar)) {
                lanzar(binding.progreso, { repo.eliminarProgreso(idProgreso) }) { finish() }
            }
            return true
        }
        return super.onOptionsItemSelected(item)
    }

    companion object {
        const val EXTRA_ID = "id_progreso"
    }
}
