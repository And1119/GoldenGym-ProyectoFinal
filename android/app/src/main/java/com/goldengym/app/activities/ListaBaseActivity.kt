package com.goldengym.app.activities

import android.os.Bundle
import android.view.View
import android.widget.AdapterView
import androidx.core.widget.doAfterTextChanged
import androidx.lifecycle.lifecycleScope
import androidx.recyclerview.widget.LinearLayoutManager
import com.goldengym.app.adapters.ItemAdapter
import com.goldengym.app.adapters.ItemUi
import com.goldengym.app.databinding.ActivityListaBinding
import com.goldengym.app.repository.Resultado
import com.goldengym.app.utils.llenar
import com.goldengym.app.utils.texto
import com.goldengym.app.utils.visible
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch

/**
 * Plantilla para todas las pantallas de listado (RF-06 listar, RF-09 buscar y filtrar por dos criterios).
 * Cada módulo solo define cómo cargar sus datos, el filtro y qué pasa al tocar un elemento.
 */
abstract class ListaBaseActivity : BaseActivity() {

    protected lateinit var binding: ActivityListaBinding
    private val adapter = ItemAdapter { onItemSeleccionado(it) }
    private var jobBusqueda: Job? = null
    private var spinnerListo = false

    /** Título de la barra superior. */
    protected abstract val titulo: String
    /** Opciones del segundo criterio de filtro (la primera debe ser "Todos"/"Todas"). */
    protected abstract val opcionesFiltro: List<String>
    /** Si el rol actual puede crear registros en este módulo. */
    protected abstract val puedeCrear: Boolean
    protected open val textoBotonNuevo: String? = null

    /** Consulta la API y convierte el resultado en filas para la lista. */
    protected abstract suspend fun cargar(buscar: String?, filtro: String): Resultado<List<ItemUi>>
    protected abstract fun onItemSeleccionado(item: ItemUi)
    protected abstract fun onNuevo()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityListaBinding.inflate(layoutInflater)
        setContentView(binding.root)
        if (!verificarSesion()) return

        configurarToolbar(binding.toolbar, titulo)
        binding.rvLista.layoutManager = LinearLayoutManager(this)
        binding.rvLista.adapter = adapter

        binding.spFiltro.llenar(this, opcionesFiltro)
        binding.spFiltro.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
            override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long) {
                if (spinnerListo) recargar() else spinnerListo = true
            }
            override fun onNothingSelected(parent: AdapterView<*>?) = Unit
        }

        // Búsqueda con espera de 400 ms para no llamar a la API en cada letra.
        binding.etBuscar.doAfterTextChanged {
            jobBusqueda?.cancel()
            jobBusqueda = lifecycleScope.launch {
                delay(400)
                recargar()
            }
        }

        binding.swipe.setOnRefreshListener { recargar() }
        binding.fabNuevo.visible(puedeCrear)
        textoBotonNuevo?.let { binding.fabNuevo.text = it }
        binding.fabNuevo.setOnClickListener { onNuevo() }
    }

    /** Se recarga al volver de un formulario para ver los cambios guardados. */
    override fun onResume() {
        super.onResume()
        if (session.haySesionValida()) recargar()
    }

    protected fun recargar() {
        val buscar = binding.etBuscar.texto.ifBlank { null }
        val filtro = binding.spFiltro.selectedItem?.toString() ?: opcionesFiltro.first()
        binding.swipe.isRefreshing = true
        lifecycleScope.launch {
            val resultado = cargar(buscar, filtro)
            binding.swipe.isRefreshing = false
            when (resultado) {
                is Resultado.Exito -> {
                    adapter.submitList(resultado.datos)
                    binding.tvVacio.visible(resultado.datos.isEmpty())
                    mostrarResumen(resultado.datos)
                }
                is Resultado.Error -> {
                    binding.tvVacio.visible(adapter.itemCount == 0)
                    manejarError(resultado)
                }
            }
        }
    }

    /** Las pantallas pueden mostrar un texto de resumen sobre la lista (ej. total de registros). */
    protected open fun resumen(items: List<ItemUi>): String? = "${items.size} registro(s)"

    private fun mostrarResumen(items: List<ItemUi>) {
        val texto = resumen(items)
        binding.tvResumen.text = texto
        binding.tvResumen.visible(!texto.isNullOrBlank())
    }

    /** Devuelve null si se eligió la primera opción ("Todos"), para no filtrar. */
    protected fun filtroOpcional(filtro: String): String? = if (filtro == opcionesFiltro.first()) null else filtro
}
