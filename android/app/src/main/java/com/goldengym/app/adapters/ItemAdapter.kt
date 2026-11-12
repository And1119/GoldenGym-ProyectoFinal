package com.goldengym.app.adapters

import android.content.res.ColorStateList
import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.annotation.ColorRes
import androidx.core.content.ContextCompat
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.goldengym.app.R
import com.goldengym.app.databinding.ItemRegistroBinding
import com.goldengym.app.utils.visible

/** Fila genérica que usan todas las listas (usuarios, membresías, rutinas, etc.). */
data class ItemUi(
    val id: Int,
    val titulo: String,
    val subtitulo: String = "",
    val detalle: String = "",
    val etiqueta: String? = null,
    @ColorRes val colorEtiqueta: Int = R.color.gris,
    val dato: Any? = null
)

class ItemAdapter(private val alSeleccionar: (ItemUi) -> Unit) :
    ListAdapter<ItemUi, ItemAdapter.VH>(DIFF) {

    class VH(val binding: ItemRegistroBinding) : RecyclerView.ViewHolder(binding.root)

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): VH =
        VH(ItemRegistroBinding.inflate(LayoutInflater.from(parent.context), parent, false))

    override fun onBindViewHolder(holder: VH, position: Int) {
        val item = getItem(position)
        with(holder.binding) {
            tvTitulo.text = item.titulo
            tvSubtitulo.text = item.subtitulo
            tvSubtitulo.visible(item.subtitulo.isNotBlank())
            tvDetalle.text = item.detalle
            tvDetalle.visible(item.detalle.isNotBlank())
            tvEtiqueta.visible(item.etiqueta != null)
            tvEtiqueta.text = item.etiqueta
            tvEtiqueta.backgroundTintList =
                ColorStateList.valueOf(ContextCompat.getColor(root.context, item.colorEtiqueta))
            root.setOnClickListener { alSeleccionar(item) }
        }
    }

    companion object {
        private val DIFF = object : DiffUtil.ItemCallback<ItemUi>() {
            override fun areItemsTheSame(a: ItemUi, b: ItemUi) = a.id == b.id
            override fun areContentsTheSame(a: ItemUi, b: ItemUi) = a == b
        }
    }
}

/** Indicadores del Home (tarjetas título / valor). */
class IndicadorAdapter : RecyclerView.Adapter<IndicadorAdapter.VH>() {
    private var datos: List<Pair<String, String>> = emptyList()

    class VH(val binding: com.goldengym.app.databinding.ItemIndicadorBinding) : RecyclerView.ViewHolder(binding.root)

    fun actualizar(nuevos: List<Pair<String, String>>) {
        datos = nuevos
        @Suppress("NotifyDataSetChanged")
        notifyDataSetChanged()
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int) =
        VH(com.goldengym.app.databinding.ItemIndicadorBinding.inflate(LayoutInflater.from(parent.context), parent, false))

    override fun getItemCount() = datos.size

    override fun onBindViewHolder(holder: VH, position: Int) {
        holder.binding.tvTituloIndicador.text = datos[position].first
        holder.binding.tvValorIndicador.text = datos[position].second
    }
}
