package com.goldengym.app

import android.app.Application
import com.goldengym.app.preferences.PreferenciasUI

/** Aplica el tema guardado (claro/oscuro) apenas arranca la app. */
class GoldenGymApp : Application() {
    override fun onCreate() {
        super.onCreate()
        PreferenciasUI.get(this).aplicarTema()
    }
}
