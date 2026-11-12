package com.goldengym.app.utils

import java.text.NumberFormat
import java.time.LocalDate
import java.time.LocalDateTime
import java.time.format.DateTimeFormatter
import java.util.Locale

/** Conversión de fechas y números entre el formato de la API (ISO) y el que ve el usuario. */
object Formatos {

    private val LOCALE_CO: Locale = Locale.forLanguageTag("es-CO")
    private val FECHA_UI = DateTimeFormatter.ofPattern("dd/MM/yyyy", LOCALE_CO)
    private val FECHA_HORA_UI = DateTimeFormatter.ofPattern("dd/MM/yyyy hh:mm a", LOCALE_CO)
    private val HORA_UI = DateTimeFormatter.ofPattern("hh:mm a", LOCALE_CO)

    /** "2026-11-12" -> "12/11/2026" */
    fun fecha(iso: String?): String {
        if (iso.isNullOrBlank()) return "—"
        return runCatching { LocalDate.parse(iso.take(10)).format(FECHA_UI) }.getOrDefault(iso)
    }

    /** "2026-11-12T18:05:30.123" -> "12/11/2026 06:05 p. m." */
    fun fechaHora(iso: String?): String {
        if (iso.isNullOrBlank()) return "—"
        return runCatching { LocalDateTime.parse(limpiarFechaHora(iso)).format(FECHA_HORA_UI) }.getOrDefault(iso)
    }

    fun hora(iso: String?): String {
        if (iso.isNullOrBlank()) return "—"
        return runCatching { LocalDateTime.parse(limpiarFechaHora(iso)).format(HORA_UI) }.getOrDefault(iso)
    }

    /** LocalDate -> "2026-11-12" (formato que espera la API). */
    fun iso(fecha: LocalDate): String = fecha.toString()

    fun hoyIso(): String = LocalDate.now().toString()

    /** 65000.0 -> "$ 65.000" */
    fun dinero(valor: Double): String {
        val nf = NumberFormat.getCurrencyInstance(LOCALE_CO)
        nf.maximumFractionDigits = 0
        return nf.format(valor)
    }

    fun numero(valor: Double): String =
        if (valor % 1.0 == 0.0) valor.toLong().toString() else String.format(Locale.US, "%.1f", valor)

    fun minutos(min: Int?): String = when {
        min == null -> "En el gimnasio"
        min < 60 -> "$min min"
        else -> "${min / 60} h ${min % 60} min"
    }

    /** Quita zona horaria y recorta fracciones de segundo largas que envía .NET. */
    private fun limpiarFechaHora(iso: String): String {
        val sinZona = iso.removeSuffix("Z").substringBefore("+")
        val partes = sinZona.split(".")
        return if (partes.size == 2) partes[0] + "." + partes[1].take(6) else sinZona
    }
}
