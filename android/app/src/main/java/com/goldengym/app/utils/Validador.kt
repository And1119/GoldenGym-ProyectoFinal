package com.goldengym.app.utils

import java.time.LocalDate

/**
 * Validaciones del lado del cliente (RNF-07). Son las mismas reglas que aplica la API,
 * para avisar al usuario antes de enviar la petición. Devuelven null si el valor es válido
 * o el mensaje de error a mostrar.
 */
object Validador {

    private val REGEX_CORREO = Regex("^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$")
    private val REGEX_PASSWORD = Regex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,64}$")
    private val REGEX_TELEFONO = Regex("^[0-9]{7,15}$")
    private val REGEX_DOCUMENTO = Regex("^[0-9]{5,20}$")

    fun requerido(valor: String?, campo: String): String? =
        if (valor.isNullOrBlank()) "$campo es obligatorio." else null

    fun nombre(valor: String?, campo: String = "El nombre"): String? = when {
        valor.isNullOrBlank() -> "$campo es obligatorio."
        valor.trim().length < 2 -> "$campo debe tener al menos 2 caracteres."
        valor.trim().length > 80 -> "$campo no puede superar 80 caracteres."
        else -> null
    }

    fun correo(valor: String?): String? = when {
        valor.isNullOrBlank() -> "El correo es obligatorio."
        !REGEX_CORREO.matches(valor.trim()) -> "El correo no tiene un formato válido."
        else -> null
    }

    fun password(valor: String?): String? = when {
        valor.isNullOrEmpty() -> "La contraseña es obligatoria."
        !REGEX_PASSWORD.matches(valor) -> "Mínimo 8 caracteres, con mayúscula, minúscula y número."
        else -> null
    }

    fun confirmacion(password: String?, confirmacion: String?): String? =
        if (password != confirmacion) "Las contraseñas no coinciden." else null

    /** Teléfono opcional: si se escribe, debe tener entre 7 y 15 dígitos. */
    fun telefono(valor: String?): String? =
        if (!valor.isNullOrBlank() && !REGEX_TELEFONO.matches(valor.trim())) "El teléfono debe tener entre 7 y 15 dígitos." else null

    /** Documento opcional: si se escribe, debe tener entre 5 y 20 dígitos. */
    fun documento(valor: String?): String? =
        if (!valor.isNullOrBlank() && !REGEX_DOCUMENTO.matches(valor.trim())) "El documento debe tener entre 5 y 20 dígitos." else null

    fun enteroEnRango(valor: String?, campo: String, min: Int, max: Int): String? {
        val n = valor?.trim()?.toIntOrNull() ?: return "$campo debe ser un número entero."
        return if (n < min || n > max) "$campo debe estar entre $min y $max." else null
    }

    fun decimalEnRango(valor: String?, campo: String, min: Double, max: Double, opcional: Boolean = false): String? {
        if (valor.isNullOrBlank()) return if (opcional) null else "$campo es obligatorio."
        val n = valor.trim().replace(',', '.').toDoubleOrNull() ?: return "$campo debe ser un número."
        return if (n < min || n > max) "$campo debe estar entre ${Formatos.numero(min)} y ${Formatos.numero(max)}." else null
    }

    /** Fecha opcional en formato ISO (yyyy-MM-dd) que no puede ser futura. */
    fun fechaNoFutura(iso: String?, campo: String, hoy: LocalDate = LocalDate.now()): String? {
        if (iso.isNullOrBlank()) return null
        val fecha = runCatching { LocalDate.parse(iso) }.getOrNull() ?: return "$campo no es una fecha válida."
        return if (fecha.isAfter(hoy)) "$campo no puede ser futura." else null
    }

    fun fechaNoPasada(iso: String?, campo: String, hoy: LocalDate = LocalDate.now()): String? {
        if (iso.isNullOrBlank()) return "$campo es obligatoria."
        val fecha = runCatching { LocalDate.parse(iso) }.getOrNull() ?: return "$campo no es una fecha válida."
        return if (fecha.isBefore(hoy)) "$campo no puede ser anterior a hoy." else null
    }
}
