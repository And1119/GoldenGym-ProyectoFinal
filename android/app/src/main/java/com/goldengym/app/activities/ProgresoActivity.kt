package com.goldengym.app.activities

import android.content.Intent
import com.goldengym.app.R
import com.goldengym.app.adapters.ItemUi
import com.goldengym.app.models.Progreso
import com.goldengym.app.repository.Resultado
import com.goldengym.app.repository.map
import com.goldengym.app.utils.Formatos
import com.google.android.material.dialog.MaterialAlertDialogBuilder
import java.time.LocalDate

/** Progreso físico (peso, % grasa, IMC). Filtros: periodo (API) + nombre del cliente. */
class ProgresoActivity : ListaBaseActivity() {

    override val titulo: String get() = getString(R.string.progreso)
    override val opcionesFiltro = listOf("Todo el historial", "Últimos 30 días", "Últimos 90 días", "Este año")
    override val puedeCrear: Boolean get() = session.esPersonal

    override suspend fun cargar(buscar: String?, filtro: String): Resultado<List<ItemUi>> {
        val hoy = LocalDate.now()
        val desde = when (filtro) {
            "Últimos 30 días" -> hoy.minusDays(30)
            "Últimos 90 días" -> hoy.minusDays(90)
            "Este año" -> hoy.withDayOfYear(1)
            else -> null
        }
        return repo.progresos(desde?.toString(), null).map { lista ->
            lista.filter { buscar == null || it.nombreCliente.contains(buscar, ignoreCase = true) }
                .map { p ->
                    ItemUi(
                        id = p.idProgreso,
                        titulo = if (session.esCliente) Formatos.fecha(p.fecha) else "${p.nombreCliente} · ${Formatos.fecha(p.fecha)}",
                        subtitulo = listOfNotNull(
                            "Peso ${Formatos.numero(p.pesoKg)} kg",
                            p.grasaCorporal?.let { "Grasa ${Formatos.numero(it)} %" },
                            p.imc?.let { "IMC ${Formatos.numero(it)}" }
                        ).joinToString(" · "),
                        detalle = p.observaciones.orEmpty(),
                        etiqueta = p.nombreEntrenador?.split(" ")?.firstOrNull(),
                        colorEtiqueta = R.color.dorado_oscuro,
                        dato = p
                    )
                }
        }
    }

    /** Para el cliente: diferencia de peso entre el primer y el último registro del periodo. */
    override fun resumen(items: List<ItemUi>): String {
        val registros = items.mapNotNull { it.dato as? Progreso }.sortedBy { it.fecha }
        if (!session.esCliente || registros.size < 2) return "${items.size} registro(s)"
        val cambio = registros.last().pesoKg - registros.first().pesoKg
        val signo = if (cambio > 0) "+" else ""
        return "${items.size} registro(s) · cambio de peso: $signo${Formatos.numero(cambio)} kg"
    }

    override fun onItemSeleccionado(item: ItemUi) {
        val p = item.dato as? Progreso ?: return
        if (session.esPersonal) {
            startActivity(Intent(this, ProgresoFormActivity::class.java).putExtra(ProgresoFormActivity.EXTRA_ID, p.idProgreso))
            return
        }
        MaterialAlertDialogBuilder(this)
            .setTitle("Medición del ${Formatos.fecha(p.fecha)}")
            .setMessage(
                "Peso: ${Formatos.numero(p.pesoKg)} kg\n" +
                "Estatura: ${p.estaturaCm?.let { Formatos.numero(it) + " cm" } ?: "—"}\n" +
                "Grasa corporal: ${p.grasaCorporal?.let { Formatos.numero(it) + " %" } ?: "—"}\n" +
                "IMC: ${p.imc?.let { Formatos.numero(it) } ?: "—"}\n" +
                "Entrenador: ${p.nombreEntrenador ?: "—"}\n\n${p.observaciones.orEmpty()}"
            )
            .setPositiveButton(R.string.aceptar, null)
            .show()
    }

    override fun onNuevo() {
        startActivity(Intent(this, ProgresoFormActivity::class.java))
    }
}
