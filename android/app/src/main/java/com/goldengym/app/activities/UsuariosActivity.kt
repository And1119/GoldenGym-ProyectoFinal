package com.goldengym.app.activities

import android.content.Intent
import com.goldengym.app.R
import com.goldengym.app.adapters.ItemUi
import com.goldengym.app.models.Roles
import com.goldengym.app.repository.Resultado
import com.goldengym.app.repository.map

/** Administrador: todos los usuarios. Entrenador: solo clientes (la API lo restringe). Filtros: texto + rol. */
class UsuariosActivity : ListaBaseActivity() {

    override val titulo: String get() = getString(if (session.esAdmin) R.string.usuarios else R.string.clientes)
    override val opcionesFiltro: List<String>
        get() = if (session.esAdmin) listOf("Todos los roles") + Roles.TODOS else listOf("Activos e inactivos", "Solo activos")
    override val puedeCrear: Boolean get() = session.esAdmin

    override suspend fun cargar(buscar: String?, filtro: String): Resultado<List<ItemUi>> {
        val rol = if (session.esAdmin) filtroOpcional(filtro) else null
        val activo = if (!session.esAdmin && filtro == "Solo activos") true else null
        return repo.usuarios(buscar, rol, activo).map { lista ->
            lista.map { u ->
                ItemUi(
                    id = u.idUsuario,
                    titulo = u.nombreCompleto,
                    subtitulo = u.correo,
                    detalle = listOfNotNull(u.documento?.let { "Doc. $it" }, u.telefono?.let { "Tel. $it" }).joinToString(" · "),
                    etiqueta = if (u.activo) u.rol else "Inactivo",
                    colorEtiqueta = when {
                        !u.activo -> R.color.gris
                        u.rol == Roles.ADMINISTRADOR -> R.color.rojo
                        u.rol == Roles.ENTRENADOR -> R.color.azul
                        else -> R.color.verde
                    }
                )
            }
        }
    }

    override fun onItemSeleccionado(item: ItemUi) {
        startActivity(Intent(this, UsuarioFormActivity::class.java).putExtra(UsuarioFormActivity.EXTRA_ID, item.id))
    }

    override fun onNuevo() {
        startActivity(Intent(this, UsuarioFormActivity::class.java))
    }
}
