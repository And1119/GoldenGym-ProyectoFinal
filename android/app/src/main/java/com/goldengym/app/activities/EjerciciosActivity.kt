package com.goldengym.app.activities

import android.content.Intent
import com.goldengym.app.R
import com.goldengym.app.adapters.ItemUi
import com.goldengym.app.models.GruposMusculares
import com.goldengym.app.repository.Resultado
import com.goldengym.app.repository.map

/** Catálogo de ejercicios. Filtros: texto + grupo muscular. Gestión: Administrador y Entrenador. */
class EjerciciosActivity : ListaBaseActivity() {

    override val titulo: String get() = getString(R.string.ejercicios)
    override val opcionesFiltro = listOf("Todos los grupos") + GruposMusculares.TODOS
    override val puedeCrear: Boolean get() = session.esPersonal

    override suspend fun cargar(buscar: String?, filtro: String): Resultado<List<ItemUi>> =
        repo.ejercicios(buscar, filtroOpcional(filtro), incluirInactivos = session.esPersonal).map { lista ->
            lista.map { e ->
                ItemUi(
                    id = e.idEjercicio,
                    titulo = e.nombre,
                    subtitulo = e.grupoMuscular,
                    detalle = e.descripcion.orEmpty(),
                    etiqueta = if (e.activo) null else "Inactivo",
                    colorEtiqueta = R.color.gris
                )
            }
        }

    override fun onItemSeleccionado(item: ItemUi) {
        if (session.esPersonal) {
            startActivity(Intent(this, EjercicioFormActivity::class.java).putExtra(EjercicioFormActivity.EXTRA_ID, item.id))
        } else {
            mostrarMensaje("${item.titulo}: ${item.detalle.ifBlank { item.subtitulo }}")
        }
    }

    override fun onNuevo() {
        startActivity(Intent(this, EjercicioFormActivity::class.java))
    }
}
