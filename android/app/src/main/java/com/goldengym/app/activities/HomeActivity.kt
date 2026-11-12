package com.goldengym.app.activities

import android.content.Intent
import android.os.Bundle
import android.view.LayoutInflater
import android.view.Menu
import android.view.MenuItem
import android.view.ViewGroup
import androidx.annotation.DrawableRes
import androidx.recyclerview.widget.GridLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.goldengym.app.R
import com.goldengym.app.adapters.IndicadorAdapter
import com.goldengym.app.databinding.ActivityHomeBinding
import com.goldengym.app.databinding.ItemModuloBinding
import com.goldengym.app.models.Dashboard
import com.goldengym.app.utils.Formatos
import com.goldengym.app.utils.visible

/** RF-04: Home con el perfil autenticado, indicadores y los módulos permitidos según el rol. */
class HomeActivity : BaseActivity() {

    private lateinit var binding: ActivityHomeBinding
    private val indicadores = IndicadorAdapter()

    private data class Modulo(val titulo: String, @DrawableRes val icono: Int, val destino: Class<*>)

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityHomeBinding.inflate(layoutInflater)
        setContentView(binding.root)
        if (!verificarSesion()) return

        configurarToolbar(binding.toolbar, getString(R.string.app_name), conAtras = false)

        binding.rvIndicadores.layoutManager = GridLayoutManager(this, 2)
        binding.rvIndicadores.adapter = indicadores
        binding.rvModulos.layoutManager = GridLayoutManager(this, 2)
        binding.rvModulos.adapter = ModuloAdapter(modulosDelRol())

        binding.swipe.setOnRefreshListener { cargar() }
        binding.btnAsistenciaRapida.setOnClickListener { registrarEntrada() }

        // Datos locales mientras responde la API.
        binding.tvNombre.text = session.nombre
        binding.tvCorreo.text = session.correo
        binding.chipRol.text = session.rol
    }

    override fun onResume() {
        super.onResume()
        if (session.haySesionValida()) cargar()
    }

    /** Cada rol ve solo los módulos que la API le permite usar (RF-03). */
    private fun modulosDelRol(): List<Modulo> {
        val lista = mutableListOf(
            Modulo(getString(R.string.membresias), R.drawable.ic_membresia, MembresiasActivity::class.java),
            Modulo(getString(R.string.rutinas), R.drawable.ic_rutina, RutinasActivity::class.java),
            Modulo(getString(R.string.asistencia), R.drawable.ic_asistencia, AsistenciaActivity::class.java),
            Modulo(getString(R.string.progreso), R.drawable.ic_progreso, ProgresoActivity::class.java),
            Modulo(getString(R.string.ejercicios), R.drawable.ic_ejercicio, EjerciciosActivity::class.java),
            Modulo(getString(R.string.planes), R.drawable.ic_plan, PlanesActivity::class.java)
        )
        if (session.esPersonal) {
            val titulo = if (session.esAdmin) getString(R.string.usuarios) else getString(R.string.clientes)
            lista.add(Modulo(titulo, R.drawable.ic_usuarios, UsuariosActivity::class.java))
        }
        lista.add(Modulo(getString(R.string.perfil), R.drawable.ic_persona, PerfilActivity::class.java))
        return lista
    }

    private fun cargar() {
        binding.swipe.isRefreshing = true
        lanzar(null, { repo.dashboard() }, alError = {
            binding.swipe.isRefreshing = false
            manejarError(it)
        }) { dashboard ->
            binding.swipe.isRefreshing = false
            mostrar(dashboard)
        }
    }

    private fun mostrar(d: Dashboard) {
        binding.tvNombre.text = d.usuario.nombreCompleto
        binding.tvCorreo.text = d.usuario.correo
        binding.chipRol.text = d.usuario.rol
        indicadores.actualizar(d.indicadores.map { it.titulo to it.valor })

        binding.cardMembresia.visible(session.esCliente)
        if (session.esCliente) {
            val m = d.membresiaVigente
            if (m != null) {
                binding.tvMembresiaTitulo.text = "${m.nombrePlan} · ${m.estado}"
                binding.tvMembresiaDetalle.text =
                    "Del ${Formatos.fecha(m.fechaInicio)} al ${Formatos.fecha(m.fechaFin)} · quedan ${m.diasRestantes} día(s)"
                binding.btnAsistenciaRapida.visible(true)
            } else {
                binding.tvMembresiaTitulo.text = getString(R.string.membresias)
                binding.tvMembresiaDetalle.text = getString(R.string.sin_membresia)
                binding.btnAsistenciaRapida.visible(false)
            }
        }
    }

    private fun registrarEntrada() {
        lanzar(null, { repo.registrarEntrada(null) }) { a ->
            mostrarMensaje("Entrada registrada a las ${Formatos.hora(a.fechaEntrada)}. ¡Buen entrenamiento!")
            cargar()
        }
    }

    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        menuInflater.inflate(R.menu.menu_home, menu)
        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean = when (item.itemId) {
        R.id.accion_perfil -> {
            startActivity(Intent(this, PerfilActivity::class.java)); true
        }
        R.id.accion_salir -> {
            confirmar(getString(R.string.cerrar_sesion), getString(R.string.confirmar_salir), getString(R.string.cerrar_sesion)) {
                authRepo.cerrarSesion() // RF-12
                irALogin("Sesión cerrada correctamente.")
            }
            true
        }
        else -> super.onOptionsItemSelected(item)
    }

    private inner class ModuloAdapter(private val modulos: List<Modulo>) : RecyclerView.Adapter<ModuloAdapter.VH>() {
        inner class VH(val b: ItemModuloBinding) : RecyclerView.ViewHolder(b.root)

        override fun onCreateViewHolder(parent: ViewGroup, viewType: Int) =
            VH(ItemModuloBinding.inflate(LayoutInflater.from(parent.context), parent, false))

        override fun getItemCount() = modulos.size

        override fun onBindViewHolder(holder: VH, position: Int) {
            val m = modulos[position]
            holder.b.tvModulo.text = m.titulo
            holder.b.ivIcono.setImageResource(m.icono)
            holder.b.root.setOnClickListener { startActivity(Intent(this@HomeActivity, m.destino)) }
        }
    }
}
