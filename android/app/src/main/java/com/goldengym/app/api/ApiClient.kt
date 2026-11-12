package com.goldengym.app.api

import android.content.Context
import com.goldengym.app.BuildConfig
import com.goldengym.app.preferences.PreferenciasUI
import com.goldengym.app.preferences.SessionManager
import okhttp3.Interceptor
import okhttp3.OkHttpClient
import okhttp3.Response
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

/** Agrega "Authorization: Bearer <token>" a cada petición cuando hay sesión. */
class AuthInterceptor(private val session: SessionManager) : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val token = session.token
        val request = if (token.isNullOrBlank()) chain.request()
        else chain.request().newBuilder().header("Authorization", "Bearer $token").build()
        return chain.proceed(request)
    }
}

/**
 * Crea (y reutiliza) la instancia de Retrofit. Si el usuario cambia la URL del servidor
 * en la configuración, se reconstruye el cliente.
 */
object ApiClient {

    @Volatile private var api: GymApi? = null
    @Volatile private var urlActual: String? = null

    fun get(context: Context): GymApi {
        val url = PreferenciasUI.get(context).urlServidor
        val existente = api
        if (existente != null && url == urlActual) return existente

        return synchronized(this) {
            val logging = HttpLoggingInterceptor().apply {
                level = if (BuildConfig.DEBUG) HttpLoggingInterceptor.Level.BODY else HttpLoggingInterceptor.Level.NONE
                redactHeader("Authorization") // nunca imprimir el token en el log
            }
            val http = OkHttpClient.Builder()
                .addInterceptor(AuthInterceptor(SessionManager.get(context)))
                .addInterceptor(logging)
                .connectTimeout(10, TimeUnit.SECONDS)
                .readTimeout(15, TimeUnit.SECONDS)
                .writeTimeout(15, TimeUnit.SECONDS)
                .build()

            Retrofit.Builder()
                .baseUrl(url)
                .client(http)
                .addConverterFactory(GsonConverterFactory.create())
                .build()
                .create(GymApi::class.java)
                .also {
                    api = it
                    urlActual = url
                }
        }
    }
}
