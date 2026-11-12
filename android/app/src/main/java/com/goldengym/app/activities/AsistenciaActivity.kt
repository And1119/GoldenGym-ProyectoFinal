package com.goldengym.app.activities

import com.goldengym.app.R
import com.goldengym.app.adapters.ItemUi
import com.goldengym.app.models.Asistencia
import com.goldengym.app.repository.Resultado
import com.goldengym.app.repository.map
import com.goldengym.app.utils.Formatos
import com.google.android.material.dialog.MaterialAlertDialogBuilder
import java.time.LocalDate

/**
 * Control de asistencia. Cliente: registra su entrada/salida y ve su historial.
 * Personal: registra la entrada de cualquier cliente. Filtros: periodo (API) + nombre del cliente.
 */
class AsistenciaActivity : ListaBaseActivity() {

    override val titulo: String get() = getString(R.string.asistencia)
    override val opcionesFiltro = listOf("Este mes", "Hoy", "Últimos 7 días", "Todo el historial")
    override val puedeCrear = true
    override val textoBotonNuevo: String get() = getString(R.string.registrar_entrada)

    override suspend fun cargar(buscar: String?, filtro: String): Resultado<List<ItemUi>> {
        val hoy = LocalDate.now()
        val desde = when (filtro) {
            "Hoy" -> hoy
            "Últimos 7 días" -> hoy.minusDays(6)
            "Este mes" -> hoy.withDayOfMonth(1)
            else -> null
        }
        return repo.asistencias(desde?.toString(), hoy.toString()).map { lista ->
            lista.filter { buscar == null || it.nombreCliente.contains(buscar, ignoreCase = true) }
                .map { a ->
                    ItemUi(
                        id = a.idAsistencia,
                        titulo = if (session.esCliente) Formatos.fecha(a.fechaEntrada) else a.nombreCliente,
                        subtitulo = "Entrada: ${Formatos.fechaHora(a.fechaEntrada)}" +
                                (a.fechaSalida?.let { "\nSalida: ${Formatos.fechaHora(it)}" } ?: ""),
                        detalle = a.registradoPor?.let { "Registrado por $it" }.orEmpty(),
                        etiqueta = Formatos.minutos(a.minutosEnGimnasio),
                        colorEtiqueta = if (a.fechaSalida == null) R.color.verde else R.color.gris,
                        dato = a
                    )
                }
        }
    }

    override fun resumen(items: List<ItemUi>): String {
        val enGimnasio = items.count { (it.dato as? Asistencia)?.fechaSalida == null }
        return "${items.size} asistencia(s) · $enGimnasio en el gimnasio ahora"
    }

    override fun onItemSeleccionado(item: ItemUi) {
        val a = item.dato as? Asistencia ?: return
        val opciones = mutableListOf<Pair<String, () -> Unit>>()
        if (a.fechaSalida == null) opciones.add(getString(R.string.registrar_salida) to { registrarSalida(a) })
        if (session.esAdmin) opciones.add(getString(R.string.eliminar) to { eliminar(a) })
        if (opciones.isEmpty()) {
            mostrarMensaje("Asistencia cerrada: ${Formatos.minutos(a.minutosEnGimnasio)} en el gimnasio.")
            return
        }
        MaterialAlertDialogBuilder(this)
            .setTitle(item.titulo)
            .setItems(opciones.map { it.first }.toTypedArray()) { _, i -> opciones[i].second() }
            .setNegativeButton(R.string.cancelar, null)
            .show()
    }

    /** Cliente: registra su propia entrada. Personal: elige el cliente que ingresa. */
    override fun onNuevo() {
        if (session.esCliente) {
            registrarEntrada(null)
            return
        }
        lanzar(null, { repo.clientesActivos() }) { clientes ->
            if (clientes.isEmpty()) {
                mostrarMensaje("No hay clientes activos.")
                return@lanzar
            }
            MaterialAlertDialogBuilder(this)
                .setTitle("¿Qué cliente ingresa?")
                .setItems(clientes.map { it.nombreCompleto }.toTypedArray()) { _, i -> registrarEntrada(clientes[i].idUsuario) }
                .setNegativeButton(R.string.cancelar, null)
                .show()
        }
    }

    private fun registrarEntrada(idCliente: Int?) {
        lanzar(null, { repo.registrarEntrada(idCliente) }) { a ->
            mostrarMensaje("Entrada de ${a.nombreCliente} registrada a las ${Formatos.hora(a.fechaEntrada)}.")
            recargar()
        }
    }

    private fun registrarSalida(a: Asistencia) {
        lanzar(null, { repo.registrarSalida(a.idAsistencia) }) { cerrada ->
            mostrarMensaje("Salida registrada. Tiempo en el gimnasio: ${Formatos.minutos(cerrada.minutosEnGimnasio)}.")
            recargar()
        }
    }

    private fun eliminar(a: Asistencia) {
        confirmar(getString(R.string.eliminar), "¿Eliminar este registro de asistencia de ${a.nombreCliente}?", getString(R.string.eliminar)) {
            lanzar(null, { repo.eliminarAsistencia(a.idAsistencia) }) {
                mostrarMensaje("Registro eliminado.")
                recargar()
            }
        }
    }
}
