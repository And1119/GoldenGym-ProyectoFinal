package com.goldengym.app

import com.goldengym.app.utils.Formatos
import com.goldengym.app.utils.Validador
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertNull
import org.junit.Test
import java.time.LocalDate

/** Pruebas unitarias de las validaciones del cliente (RNF-07). Ejecutar: ./gradlew test */
class ValidadorTest {

    @Test
    fun correo_valido_e_invalido() {
        assertNull(Validador.correo("andres@goldengym.com"))
        assertNull(Validador.correo("  usuario@correo.com.co "))
        assertNotNull(Validador.correo(""))
        assertNotNull(Validador.correo("usuariocorreo.com"))
        assertNotNull(Validador.correo("usuario@"))
        assertNotNull(Validador.correo("usuario @correo.com"))
    }

    @Test
    fun password_exige_mayuscula_minuscula_numero_y_8_caracteres() {
        assertNull(Validador.password("Cliente123*"))
        assertNotNull(Validador.password("corta1A"))
        assertNotNull(Validador.password("sinmayuscula1"))
        assertNotNull(Validador.password("SINMINUSCULA1"))
        assertNotNull(Validador.password("SinNumeros"))
    }

    @Test
    fun nombre_minimo_dos_caracteres() {
        assertNull(Validador.nombre("Ana"))
        assertNotNull(Validador.nombre("A"))
        assertNotNull(Validador.nombre("   "))
    }

    @Test
    fun campos_opcionales() {
        assertNull(Validador.telefono(""))
        assertNull(Validador.telefono("3001234567"))
        assertNotNull(Validador.telefono("30A"))
        assertNull(Validador.documento(null))
        assertNotNull(Validador.documento("12"))
    }

    @Test
    fun rangos_numericos() {
        assertNull(Validador.enteroEnRango("4", "Series", 1, 20))
        assertNotNull(Validador.enteroEnRango("0", "Series", 1, 20))
        assertNotNull(Validador.enteroEnRango("abc", "Series", 1, 20))
        assertNull(Validador.decimalEnRango("76,4", "Peso", 20.0, 400.0))
        assertNotNull(Validador.decimalEnRango("10", "Peso", 20.0, 400.0))
        assertNull(Validador.decimalEnRango("", "Estatura", 50.0, 250.0, opcional = true))
    }

    @Test
    fun fechas() {
        val hoy = LocalDate.of(2026, 11, 12)
        assertNull(Validador.fechaNoFutura("2026-11-12", "Fecha", hoy))
        assertNotNull(Validador.fechaNoFutura("2026-11-13", "Fecha", hoy))
        assertNull(Validador.fechaNoPasada("2026-11-12", "Fecha", hoy))
        assertNotNull(Validador.fechaNoPasada("2026-11-11", "Fecha", hoy))
    }

    @Test
    fun formatos_de_fecha_y_tiempo() {
        assertEquals("12/11/2026", Formatos.fecha("2026-11-12"))
        assertEquals("12/11/2026", Formatos.fecha("2026-11-12T08:30:00"))
        assertEquals("—", Formatos.fecha(null))
        assertEquals("1 h 35 min", Formatos.minutos(95))
        assertEquals("En el gimnasio", Formatos.minutos(null))
        assertEquals("76.4", Formatos.numero(76.4))
        assertEquals("65", Formatos.numero(65.0))
    }
}
