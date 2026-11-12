package com.goldengym.app.utils

import android.app.DatePickerDialog
import android.content.Context
import android.view.View
import android.widget.ArrayAdapter
import android.widget.Spinner
import com.google.android.material.textfield.TextInputEditText
import com.google.android.material.textfield.TextInputLayout
import java.time.LocalDate

/** Texto recortado de un TextInputEditText ("" si está vacío). */
val TextInputEditText.texto: String get() = text?.toString()?.trim().orEmpty()

/** Texto o null si está vacío (para campos opcionales). */
val TextInputEditText.textoONull: String? get() = texto.ifBlank { null }

/** Muestra el error bajo el campo; devuelve true si el campo es válido. */
fun TextInputLayout.validar(error: String?): Boolean {
    this.error = error
    isErrorEnabled = error != null
    return error == null
}

fun View.visible(mostrar: Boolean) {
    visibility = if (mostrar) View.VISIBLE else View.GONE
}

/**
 * Convierte un campo de texto en selector de fecha. El valor visible es dd/MM/yyyy y
 * el valor ISO (yyyy-MM-dd) se guarda en el tag del campo.
 */
fun TextInputEditText.comoSelectorDeFecha(context: Context, alSeleccionar: ((LocalDate) -> Unit)? = null) {
    isFocusable = false
    isClickable = true
    setOnClickListener {
        val actual = fechaIso?.let { runCatching { LocalDate.parse(it) }.getOrNull() } ?: LocalDate.now()
        DatePickerDialog(context, { _, anio, mes, dia ->
            val fecha = LocalDate.of(anio, mes + 1, dia)
            fijarFecha(fecha)
            alSeleccionar?.invoke(fecha)
        }, actual.year, actual.monthValue - 1, actual.dayOfMonth).show()
    }
}

var TextInputEditText.fechaIso: String?
    get() = tag as? String
    set(valor) {
        tag = valor
        setText(if (valor.isNullOrBlank()) "" else Formatos.fecha(valor))
    }

fun TextInputEditText.fijarFecha(fecha: LocalDate) {
    fechaIso = fecha.toString()
}

/** Llena un Spinner con textos simples. */
fun Spinner.llenar(context: Context, opciones: List<String>) {
    adapter = ArrayAdapter(context, android.R.layout.simple_spinner_item, opciones).also {
        it.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
    }
}

fun Spinner.seleccionar(valor: String?) {
    if (valor == null) return
    for (i in 0 until count) {
        if (getItemAtPosition(i)?.toString() == valor) {
            setSelection(i)
            return
        }
    }
}
