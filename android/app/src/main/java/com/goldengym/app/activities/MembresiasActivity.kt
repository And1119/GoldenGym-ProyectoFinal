package com.goldengym.app.activities

import android.content.Intent
import com.goldengym.app.R
import com.goldengym.app.adapters.ItemUi
import com.goldengym.app.repository.Resultado
import com.goldengym.app.repository.map
import com.goldengym.app.utils.Formatos

/** Membresías. Cliente: las suyas y "Adquirir plan". Administrador: todas. Filtros: texto + estado. */
class MembresiasActivity : ListaBaseActivity() {

    override val titulo: String get() = getString(R.string.membresias)
    override val opcionesFiltro = listOf("Todos los estados", "Vigente", "Pendiente", "Vencida", "Cancelada")
    override val puedeCrear: Boolean get() = session.esAdmin || session.esCliente
    override val textoBotonNuevo: String? get() = if (session.esCliente) "Adquirir plan" else null

    override suspend fun cargar(buscar: String?, filtro: String): Resultado<List<ItemUi>> =
        repo.membresias(filtroOpcional(filtro), buscar).map { lista ->
            lista.map { m ->
                ItemUi(
                    id = m.idMembresia,
                    titulo = if (session.esCliente) m.nombrePlan else "${m.nombreCliente} · ${m.nombrePlan}",
                    subtitulo = "${Formatos.fecha(m.fechaInicio)} → ${Formatos.fecha(m.fechaFin)}",
                    detalle = "Pagado: ${Formatos.dinero(m.precioPagado)}" +
                            if (m.diasRestantes > 0) " · ${m.diasRestantes} día(s) restantes" else "",
                    etiqueta = m.estado,
                    colorEtiqueta = when (m.estado) {
                        "Vigente" -> R.color.verde
                        "Pendiente" -> R.color.azul
                        "Vencida" -> R.color.naranja
                        else -> R.color.gris
                    }
                )
            }
        }

    override fun resumen(items: List<ItemUi>): String {
        val vigentes = items.count { it.etiqueta == "Vigente" }
        return "${items.size} membresía(s) · $vigentes vigente(s)"
    }

    override fun onItemSeleccionado(item: ItemUi) {
        startActivity(Intent(this, MembresiaFormActivity::class.java).putExtra(MembresiaFormActivity.EXTRA_ID, item.id))
    }

    override fun onNuevo() {
        startActivity(Intent(this, MembresiaFormActivity::class.java))
    }
}
