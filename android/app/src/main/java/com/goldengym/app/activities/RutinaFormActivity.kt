package com.goldengym.app.activities

import android.os.Bundle
import androidx.recyclerview.widget.LinearLayoutManager
import com.goldengym.app.R
import com.goldengym.app.adapters.ItemAdapter
import com.goldengym.app.adapters.ItemUi
import com.goldengym.app.databinding.ActivityRutinaFormBinding
import com.goldengym.app.databinding.DialogoEjercicioRutinaBinding
import com.goldengym.app.models.*
import com.goldengym.app.repository.Resultado
import com.goldengym.app.utils.*
import com.google.android.material.dialog.MaterialAlertDialogBuilder

/** Entrenador / Administrador: crear y editar rutinas con su lista de ejercicios (tabla RutinaEjercicio). */
class RutinaFormActivity : BaseActivity() {

    private lateinit var binding: ActivityRutinaFormBinding
    private var idRutina = 0
    private val esEdicion get() = idRutina > 0
    private var clientes: List<Usuario> = emptyList()
    private var catalogo: List<Ejercicio> = emptyList()
    private val detalle = mutableListOf<RutinaEjercicio>()
    private var idClientePendiente: Int? = null
    private val adapter = ItemAdapter { item -> opcionesEjercicio(item.id) }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityRutinaFormBinding.inflate(layoutInflater)
        setContentView(binding.root)
        if (!verificarSesion()) return

        idRutina = intent.getIntExtra(EXTRA_ID, 0)
        configurarToolbar(binding.toolbar, if (esEdicion) "Editar rutina" else "Nueva rutina")
        binding.spNivel.llenar(this, Niveles.TODOS)
        binding.rvEjercicios.layoutManager = LinearLayoutManager(this)
        binding.rvEjercicios.adapter = adapter
        binding.btnAgregarEjercicio.setOnClickListener { dialogoEjercicio(null) }
        binding.btnGuardar.setOnClickListener { guardar() }
        refrescarDetalle()

        lanzar(binding.progreso, { repo.clientesActivos() }) { lista ->
            clientes = lista
            binding.spCliente.llenar(this, lista.map { it.nombreCompleto })
            idClientePendiente?.let { seleccionarCliente(it) }
        }
        lanzar(null, { repo.ejercicios() }) { catalogo = it }
        if (esEdicion) lanzar(binding.progreso, { repo.rutina(idRutina) }) { mostrar(it) }
    }

    private fun mostrar(r: Rutina) {
        binding.etNombre.setText(r.nombre)
        binding.etObjetivo.setText(r.objetivo.orEmpty())
        binding.spNivel.seleccionar(r.nivel)
        binding.swActiva.isChecked = r.activa
        idClientePendiente = r.idCliente
        seleccionarCliente(r.idCliente)
        detalle.clear()
        detalle.addAll(r.ejercicios.sortedBy { it.orden })
        refrescarDetalle()
    }

    private fun seleccionarCliente(id: Int) {
        val pos = clientes.indexOfFirst { it.idUsuario == id }
        if (pos >= 0) binding.spCliente.setSelection(pos)
    }

    private fun refrescarDetalle() {
        adapter.submitList(detalle.mapIndexed { i, e ->
            ItemUi(
                id = e.idEjercicio,
                titulo = "${i + 1}. ${e.nombreEjercicio}",
                subtitulo = "${e.series} × ${e.repeticiones} · descanso ${e.descansoSeg} s",
                etiqueta = e.grupoMuscular,
                colorEtiqueta = R.color.dorado_oscuro
            )
        })
        binding.tvSinEjercicios.visible(detalle.isEmpty())
        binding.tvTituloEjercicios.text = "${getString(R.string.ejercicios)} (${detalle.size})"
    }

    private fun opcionesEjercicio(idEjercicio: Int) {
        val existente = detalle.firstOrNull { it.idEjercicio == idEjercicio } ?: return
        MaterialAlertDialogBuilder(this)
            .setTitle(existente.nombreEjercicio)
            .setItems(arrayOf(getString(R.string.editar), "Quitar de la rutina")) { _, i ->
                if (i == 0) dialogoEjercicio(existente) else {
                    detalle.remove(existente)
                    refrescarDetalle()
                }
            }
            .show()
    }

    /** Agrega o edita un ejercicio de la rutina, validando series, repeticiones y descanso. */
    private fun dialogoEjercicio(existente: RutinaEjercicio?) {
        val activos = catalogo.filter { it.activo }
        if (activos.isEmpty()) {
            mostrarMensaje("No hay ejercicios activos en el catálogo.")
            return
        }
        val d = DialogoEjercicioRutinaBinding.inflate(layoutInflater)
        d.spEjercicio.llenar(this, activos.map { "${it.nombre} (${it.grupoMuscular})" })
        if (existente != null) {
            d.spEjercicio.setSelection(activos.indexOfFirst { it.idEjercicio == existente.idEjercicio }.coerceAtLeast(0))
            d.spEjercicio.isEnabled = false
            d.etSeries.setText(existente.series.toString())
            d.etRepeticiones.setText(existente.repeticiones.toString())
            d.etDescanso.setText(existente.descansoSeg.toString())
        }

        val dialogo = MaterialAlertDialogBuilder(this)
            .setTitle(if (existente == null) R.string.agregar_ejercicio else R.string.editar)
            .setView(d.root)
            .setNegativeButton(R.string.cancelar, null)
            .setPositiveButton(R.string.aceptar, null)
            .create()

        dialogo.setOnShowListener {
            // Se sobreescribe el botón para que el diálogo no se cierre si hay errores.
            dialogo.getButton(android.content.DialogInterface.BUTTON_POSITIVE).setOnClickListener {
                val ok = listOf(
                    d.tilSeries.validar(Validador.enteroEnRango(d.etSeries.texto, "Series", 1, 20)),
                    d.tilRepeticiones.validar(Validador.enteroEnRango(d.etRepeticiones.texto, "Repeticiones", 1, 100)),
                    d.tilDescanso.validar(Validador.enteroEnRango(d.etDescanso.texto, "Descanso", 0, 600))
                ).all { it }
                if (!ok) return@setOnClickListener

                val ejercicio = activos[d.spEjercicio.selectedItemPosition]
                if (existente == null && detalle.any { it.idEjercicio == ejercicio.idEjercicio }) {
                    mostrarMensaje("Ese ejercicio ya está en la rutina.")
                    return@setOnClickListener
                }
                val nuevo = RutinaEjercicio(
                    idEjercicio = ejercicio.idEjercicio,
                    nombreEjercicio = ejercicio.nombre,
                    grupoMuscular = ejercicio.grupoMuscular,
                    series = d.etSeries.texto.toInt(),
                    repeticiones = d.etRepeticiones.texto.toInt(),
                    descansoSeg = d.etDescanso.texto.toInt(),
                    orden = existente?.orden ?: (detalle.size + 1)
                )
                val pos = detalle.indexOfFirst { it.idEjercicio == nuevo.idEjercicio }
                if (pos >= 0) detalle[pos] = nuevo else detalle.add(nuevo)
                refrescarDetalle()
                dialogo.dismiss()
            }
        }
        dialogo.show()
    }

    private fun guardar() {
        if (!binding.tilNombre.validar(Validador.nombre(binding.etNombre.texto, "El nombre de la rutina"))) return
        val cliente = clientes.getOrNull(binding.spCliente.selectedItemPosition)
        if (cliente == null) {
            mostrarMensaje("Selecciona el cliente al que se asigna la rutina.")
            return
        }
        if (detalle.isEmpty()) {
            mostrarMensaje("La rutina debe tener al menos un ejercicio.")
            return
        }
        val req = RutinaRequest(
            nombre = binding.etNombre.texto,
            objetivo = binding.etObjetivo.textoONull,
            nivel = binding.spNivel.selectedItem.toString(),
            idCliente = cliente.idUsuario,
            activa = binding.swActiva.isChecked,
            ejercicios = detalle.mapIndexed { i, e ->
                RutinaEjercicioRequest(e.idEjercicio, e.series, e.repeticiones, e.descansoSeg, i + 1)
            }
        )
        val llamada: suspend () -> Resultado<Rutina> = {
            if (esEdicion) repo.actualizarRutina(idRutina, req) else repo.crearRutina(req)
        }
        lanzar(binding.progreso, llamada) {
            mostrarMensaje(getString(R.string.guardado_ok))
            finish()
        }
    }

    companion object {
        const val EXTRA_ID = "id_rutina"
    }
}
