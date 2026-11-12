package com.goldengym.app.activities

import android.content.Intent
import com.goldengym.app.R
import com.goldengym.app.adapters.ItemUi
import com.goldengym.app.models.Niveles
import com.goldengym.app.repository.Resultado
import com.goldengym.app.repository.map
import com.goldengym.app.utils.Formatos

/** Rutinas. Cliente: las suyas. Entrenador/Admin: todas. Filtros: texto (nombre, objetivo, cliente) + nivel. */
class RutinasActivity : ListaBaseActivity() {

    override val titulo: String get() = getString(R.string.rutinas)
    override val opcionesFiltro = listOf("Todos los niveles") + Niveles.TODOS
    override val puedeCrear: Boolean get() = session.esPersonal

    override suspend fun cargar(buscar: String?, filtro: String): Resultado<List<ItemUi>> =
        repo.rutinas(buscar, filtroOpcional(filtro), incluirInactivas = session.esPersonal).map { lista ->
            lista.map { r ->
                ItemUi(
                    id = r.idRutina,
                    titulo = r.nombre,
                    subtitulo = if (session.esCliente) "Entrenador: ${r.nombreEntrenador}" else "Cliente: ${r.nombreCliente}",
                    detalle = "${r.cantidadEjercicios} ejercicio(s) · creada el ${Formatos.fecha(r.fechaCreacion)}" +
                            (r.objetivo?.let { "\n$it" } ?: ""),
                    etiqueta = if (r.activa) r.nivel else "Inactiva",
                    colorEtiqueta = when {
                        !r.activa -> R.color.gris
                        r.nivel == "Avanzado" -> R.color.rojo
                        r.nivel == "Intermedio" -> R.color.naranja
                        else -> R.color.verde
                    }
                )
            }
        }

    override fun onItemSeleccionado(item: ItemUi) {
        startActivity(Intent(this, RutinaDetalleActivity::class.java).putExtra(RutinaDetalleActivity.EXTRA_ID, item.id))
    }

    override fun onNuevo() {
        startActivity(Intent(this, RutinaFormActivity::class.java))
    }
}
