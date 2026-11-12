package com.goldengym.app.activities

import android.content.Intent
import android.os.Bundle
import androidx.core.content.ContextCompat
import androidx.recyclerview.widget.LinearLayoutManager
import com.goldengym.app.R
import com.goldengym.app.adapters.ItemAdapter
import com.goldengym.app.adapters.ItemUi
import com.goldengym.app.databinding.ActivityRutinaDetalleBinding
import com.goldengym.app.models.Rutina
import com.goldengym.app.utils.Formatos
import com.goldengym.app.utils.visible

/**
 * RF-10: detalle de una rutina con sus ejercicios. El entrenador que la creó (o el administrador)
 * puede editarla o desactivarla; si otro entrenador lo intenta, la API responde 403.
 */
class RutinaDetalleActivity : BaseActivity() {

    private lateinit var binding: ActivityRutinaDetalleBinding
    private val adapter = ItemAdapter { }
    private var idRutina = 0

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityRutinaDetalleBinding.inflate(layoutInflater)
        setContentView(binding.root)
        if (!verificarSesion()) return

        idRutina = intent.getIntExtra(EXTRA_ID, 0)
        configurarToolbar(binding.toolbar, getString(R.string.rutinas))
        binding.rvEjercicios.layoutManager = LinearLayoutManager(this)
        binding.rvEjercicios.adapter = adapter

        binding.panelAcciones.visible(session.esPersonal)
        binding.btnEditar.setOnClickListener {
            startActivity(Intent(this, RutinaFormActivity::class.java).putExtra(RutinaFormActivity.EXTRA_ID, idRutina))
        }
        binding.btnDesactivar.setOnClickListener {
            confirmar(getString(R.string.desactivar), "La rutina dejará de mostrarse al cliente.", getString(R.string.desactivar)) {
                lanzar(binding.progreso, { repo.desactivarRutina(idRutina) }) {
                    mostrarMensaje("Rutina desactivada.")
                    finish()
                }
            }
        }
    }

    override fun onResume() {
        super.onResume()
        if (session.haySesionValida()) lanzar(binding.progreso, { repo.rutina(idRutina) }) { mostrar(it) }
    }

    private fun mostrar(r: Rutina) = with(binding) {
        tvNombre.text = r.nombre
        tvNivel.text = if (r.activa) r.nivel else "${r.nivel} · Inactiva"
        tvNivel.backgroundTintList = ContextCompat.getColorStateList(this@RutinaDetalleActivity, when (r.nivel) {
            "Avanzado" -> R.color.rojo
            "Intermedio" -> R.color.naranja
            else -> R.color.verde
        })
        tvObjetivo.text = r.objetivo ?: "Sin objetivo registrado"
        tvPersonas.text = "Cliente: ${r.nombreCliente}\nEntrenador: ${r.nombreEntrenador}\nCreada: ${Formatos.fecha(r.fechaCreacion)}"
        btnDesactivar.isEnabled = r.activa
        adapter.submitList(r.ejercicios.sortedBy { it.orden }.mapIndexed { i, e ->
            ItemUi(
                id = e.idEjercicio,
                titulo = "${i + 1}. ${e.nombreEjercicio}",
                subtitulo = "${e.series} series × ${e.repeticiones} repeticiones",
                detalle = "Descanso: ${e.descansoSeg} s",
                etiqueta = e.grupoMuscular,
                colorEtiqueta = R.color.dorado_oscuro
            )
        })
    }

    companion object {
        const val EXTRA_ID = "id_rutina"
    }
}
