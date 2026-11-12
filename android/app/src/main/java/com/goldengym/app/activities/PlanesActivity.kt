package com.goldengym.app.activities

import android.content.Intent
import com.goldengym.app.R
import com.goldengym.app.adapters.ItemUi
import com.goldengym.app.repository.Resultado
import com.goldengym.app.repository.map
import com.goldengym.app.utils.Formatos

/** Planes y tarifas del gimnasio. Filtros: texto + duración. Solo el administrador los gestiona. */
class PlanesActivity : ListaBaseActivity() {

    override val titulo: String get() = getString(R.string.planes)
    override val opcionesFiltro: List<String>
        get() = listOf("Cualquier duración", "Hasta 7 días", "Más de 7 días") +
                if (session.esAdmin) listOf("Incluir inactivos") else emptyList()
    override val puedeCrear: Boolean get() = session.esAdmin

    override suspend fun cargar(buscar: String?, filtro: String): Resultado<List<ItemUi>> {
        val incluirInactivos = session.esAdmin && filtro == "Incluir inactivos"
        return repo.planes(buscar, incluirInactivos).map { lista ->
            lista.filter {
                when (filtro) {
                    "Hasta 7 días" -> it.duracionDias <= 7
                    "Más de 7 días" -> it.duracionDias > 7
                    else -> true
                }
            }.map { p ->
                ItemUi(
                    id = p.idPlan,
                    titulo = p.nombre,
                    subtitulo = "${Formatos.dinero(p.precio)} · ${p.duracionDias} día(s)",
                    detalle = p.descripcion.orEmpty(),
                    etiqueta = if (p.activo) "Activo" else "Inactivo",
                    colorEtiqueta = if (p.activo) R.color.dorado_oscuro else R.color.gris,
                    dato = p
                )
            }
        }
    }

    override fun onItemSeleccionado(item: ItemUi) {
        if (session.esAdmin) {
            startActivity(Intent(this, PlanFormActivity::class.java).putExtra(PlanFormActivity.EXTRA_ID, item.id))
        } else {
            // Clientes y entrenadores ven el detalle y el cliente puede adquirir el plan.
            startActivity(Intent(this, MembresiaFormActivity::class.java).putExtra(MembresiaFormActivity.EXTRA_ID_PLAN, item.id))
        }
    }

    override fun onNuevo() {
        startActivity(Intent(this, PlanFormActivity::class.java))
    }
}
