# Genera los wireframes de las pantallas (estilo boceto) y los exporta a PNG con Playwright.
import asyncio, pathlib, html
CSS = """
*{box-sizing:border-box;font-family:Arial,Helvetica,sans-serif}
body{margin:0;background:#fff}
.tel{text-align:left;width:360px;height:740px;border:3px solid #333;border-radius:34px;padding:14px 10px;background:#fafafa;position:relative;overflow:hidden}
.pantalla{background:#fff;border:1px solid #bbb;border-radius:18px;height:100%;overflow:hidden;display:flex;flex-direction:column}
.barra{background:#222;color:#e0c25a;padding:13px 14px;font-weight:bold;font-size:16px;display:flex;justify-content:space-between;align-items:center}
.cuerpo{padding:12px 14px;flex:1;overflow:hidden;font-size:13px;color:#222}
.campo{border:1.5px solid #888;border-radius:6px;padding:9px 10px;margin:7px 0;color:#777;position:relative;background:#fff}
.campo.err{border-color:#c62828}.error{color:#c62828;font-size:11px;margin:-4px 0 6px 4px}
.ayuda{color:#777;font-size:11px;margin:-4px 0 6px 4px}
.btn{background:#d4af37;color:#000;font-weight:bold;text-align:center;padding:11px;border-radius:22px;margin:10px 0}
.btn2{border:1.5px solid #555;text-align:center;padding:8px;border-radius:20px;margin:6px 0;color:#333}
.link{color:#8a6d12;text-align:center;margin:8px 0;text-decoration:underline}
.logo{height:90px;border:2px dashed #aaa;border-radius:10px;display:flex;align-items:center;justify-content:center;color:#888;margin:6px 0 4px}
.card{border:1.5px solid #999;border-radius:10px;padding:9px 10px;margin:7px 0;display:flex;justify-content:space-between;gap:6px}
.card b{display:block;font-size:13px}.card small{color:#666;display:block;font-size:11px;margin-top:2px}
.tag{border-radius:10px;padding:2px 8px;font-size:10px;font-weight:bold;color:#fff;align-self:center;white-space:nowrap}
.v{background:#2e7d32}.n{background:#ef6c00}.g{background:#757575}.a{background:#1565c0}.r{background:#c62828}.o{background:#9c7c1c}
.grid{display:grid;grid-template-columns:1fr 1fr;gap:8px;margin:8px 0}
.tile{border:1.5px solid #c9a227;background:#fff7d6;border-radius:10px;padding:14px 6px;text-align:center;font-weight:bold;font-size:12px}
.kpi{border:1.5px solid #c9a227;border-radius:10px;padding:8px}.kpi b{font-size:17px;color:#9c7c1c;display:block}.kpi small{color:#666;font-size:10px}
.head{background:#1a1a1a;color:#fff;padding:12px 14px}.head .chip{display:inline-block;background:#d4af37;color:#000;border-radius:12px;padding:2px 10px;font-size:11px;margin-top:4px}
.fila{display:flex;gap:6px}.fila>*{flex:1}
.fab{position:absolute;right:26px;bottom:34px;background:#d4af37;border-radius:20px;padding:11px 16px;font-weight:bold;box-shadow:0 2px 4px #0004}
.snack{position:absolute;left:24px;right:24px;bottom:34px;background:#323232;color:#fff;border-radius:6px;padding:10px 12px;font-size:12px}
.dialog{position:absolute;left:30px;right:30px;top:230px;background:#fff;border:2px solid #333;border-radius:14px;padding:14px;box-shadow:0 0 0 2000px #0006}
.titulo{font-weight:bold;font-size:15px;margin:4px 0 6px}
.nota{font-size:11px;color:#555;background:#fff7d6;border:1px dashed #c9a227;padding:6px;border-radius:6px;margin:6px 0}
.sw{display:flex;justify-content:space-between;margin:9px 2px}.sw span:last-child{width:34px;height:18px;border-radius:9px;background:#d4af37;display:inline-block}
.sw.off span:last-child{background:#bbb}
"""
def campo(t, err=None, ayuda=None, extra=""):
    h = f'<div class="campo{" err" if err else ""}">{t}{extra}</div>'
    if err: h += f'<div class="error">⚠ {err}</div>'
    if ayuda: h += f'<div class="ayuda">{ayuda}</div>'
    return h
def card(t, s, tag=None, cls="g", s2=None):
    return f'<div class="card"><div><b>{t}</b><small>{s}</small>{f"<small>{s2}</small>" if s2 else ""}</div>{f"<span class=tag style=background:{dict(v="#2e7d32",n="#ef6c00",g="#757575",a="#1565c0",r="#c62828",o="#9c7c1c")[cls]}>{tag}</span>" if tag else ""}</div>'
def barra(t, atras=True, extra=""): return f'<div class="barra"><span>{"← " if atras else ""}{t}</span><span>{extra}</span></div>'
def filtros(b, f): return f'<div class="fila">{campo("🔍 "+b)}{campo(f+" ▾")}</div>'
P = {}
P['01_login'] = ('Login', barra('', False, '⚙') + '<div class="cuerpo"><div class="logo">LOGO GOLDEN GYM</div><div style="text-align:center;color:#8a6d12">Tu meta es nuestra meta</div>'
  + campo('Correo electrónico', 'El correo no tiene un formato válido.') + campo('Contraseña ●●●●●●', extra='<span style="float:right">👁</span>')
  + '<div>☑ Recordar mi correo</div><div class="btn">INGRESAR</div><div class="link">¿No tienes cuenta? Regístrate</div>'
  + '<div class="nota">401 → "Correo o contraseña incorrectos." · Sin conexión → "No se pudo conectar con el servidor (⚙)"</div></div>')
P['02_registro'] = ('Registro', barra('Registro de cliente') + '<div class="cuerpo">' + campo('Nombres') + campo('Apellidos')
  + campo('Correo electrónico', 'El correo ya está registrado. (409 de la API)') + campo('Teléfono (opcional)') + campo('Documento (opcional)')
  + campo('Fecha de nacimiento 📅') + campo('Contraseña', ayuda='Mínimo 8 caracteres, una mayúscula, una minúscula y un número')
  + campo('Confirmar contraseña', 'Las contraseñas no coinciden.') + '<div class="btn">CREAR MI CUENTA</div></div>')
P['03_home_cliente'] = ('Home (Cliente)', barra('Golden Gym', False, '👤 ⋮') + '<div class="head">Hola,<br><b style="font-size:18px">Andrés Cliente</b><br><small>andres.cliente@goldengym.com</small><br><span class="chip">Cliente</span></div><div class="cuerpo">'
  + '<div class="card" style="border-color:#d4af37;border-width:2px"><div><b>Plan Mensual · Vigente</b><small>Del 02/11/2026 al 01/12/2026 · quedan 20 día(s)</small><div class="btn2" style="margin-top:6px">✔ Registrar entrada</div></div></div>'
  + '<div class="grid"><div class="kpi"><b>3</b><small>Asistencias este mes</small></div><div class="kpi"><b>76.4 kg</b><small>Último peso</small></div></div>'
  + '<b>Módulos</b><div class="grid"><div class="tile">Membresías</div><div class="tile">Rutinas</div><div class="tile">Asistencia</div><div class="tile">Progreso</div><div class="tile">Ejercicios</div><div class="tile">Planes</div></div></div>')
P['04_home_admin'] = ('Home (Administrador)', barra('Golden Gym', False, '👤 ⋮') + '<div class="head">Hola,<br><b style="font-size:18px">Administrador Golden Gym</b><br><span class="chip">Administrador</span></div><div class="cuerpo">'
  + '<div class="grid"><div class="kpi"><b>3</b><small>Clientes activos</small></div><div class="kpi"><b>2</b><small>Membresías vigentes</small></div><div class="kpi"><b>0</b><small>Vencen en 3 días</small></div><div class="kpi"><b>4</b><small>Asistencias hoy</small></div></div>'
  + '<b>Módulos</b><div class="grid"><div class="tile">Membresías</div><div class="tile">Rutinas</div><div class="tile">Asistencia</div><div class="tile">Progreso</div><div class="tile">Ejercicios</div><div class="tile">Planes</div><div class="tile">Usuarios</div><div class="tile">Perfil</div></div></div>')
P['05_membresias'] = ('Lista de membresías', barra('Membresías') + '<div class="cuerpo">' + filtros('Buscar…', 'Vigente') + '<small style="color:#666">2 membresía(s) · 2 vigente(s)</small>'
  + card('Andrés Cliente · Plan Mensual', '02/11/2026 → 01/12/2026', 'Vigente', 'v', 'Pagado: $ 65.000 · 20 día(s) restantes')
  + card('Johana Saavedra · Plan Quincenal', '09/11/2026 → 23/11/2026', 'Vigente', 'v', 'Pagado: $ 55.000 · 12 día(s) restantes')
  + card('Miguel Rojas · Plan Semanal', '23/10/2026 → 29/10/2026', 'Vencida', 'n', 'Pagado: $ 40.000')
  + '</div><div class="fab">＋ Nuevo</div>')
P['06_membresia_form'] = ('Adquirir plan / nueva membresía', barra('Adquirir plan') + '<div class="cuerpo"><small style="color:#666">Plan</small>' + campo('Plan Mensual · $ 65.000 ▾')
  + '<small style="color:#666">Acceso ilimitado por 30 días, cardio-rumba…</small>' + campo('Fecha de inicio 📅  12/11/2026', 'La fecha de inicio no puede ser anterior a hoy.')
  + '<div class="card" style="display:block;border-color:#d4af37">Plan: Plan Mensual (30 día(s))<br>Desde: 12/11/2026<br>Hasta: 11/12/2026<br><b>Total: $ 65.000</b></div>'
  + '<div class="btn">CONFIRMAR COMPRA</div></div><div class="snack">El cliente ya tiene una membresía activa del 02/11/2026 al 01/12/2026. (422)</div>')
P['07_rutina_detalle'] = ('Detalle de rutina', barra('Rutinas') + '<div class="cuerpo"><div class="card" style="display:block;border-color:#d4af37"><b style="font-size:16px">Fuerza tren superior</b>'
  + '<span class="tag n" style="display:inline-block;margin:4px 0">Intermedio</span><br>Ganar fuerza en pecho, espalda y brazos<br><small>Cliente: Andrés Cliente · Entrenador: Carlos Arias</small>'
  + '<div class="fila"><div class="btn2">Editar</div><div class="btn2">Desactivar</div></div></div><b>Ejercicios</b>'
  + card('1. Press de banca', '4 series × 10 repeticiones', 'Pecho', 'o', 'Descanso: 90 s') + card('2. Jalón al pecho', '4 series × 12 repeticiones', 'Espalda', 'o', 'Descanso: 60 s')
  + card('3. Press militar', '3 series × 10 repeticiones', 'Hombros', 'o') + '</div>'
  + '<div class="dialog"><div class="titulo">Acceso denegado</div>Solo puedes modificar las rutinas que tú creaste. (403)<div style="text-align:right;color:#8a6d12;font-weight:bold;margin-top:10px">ACEPTAR</div></div>')
P['08_rutina_form'] = ('Formulario de rutina', barra('Nueva rutina') + '<div class="cuerpo">' + campo('Nombre', 'El nombre de la rutina es obligatorio.') + campo('Objetivo (opcional)')
  + '<small style="color:#666">Nivel</small>' + campo('Intermedio ▾') + '<small style="color:#666">Cliente</small>' + campo('Andrés Cliente ▾') + '<div class="sw"><span>Activa</span><span></span></div>'
  + '<div class="fila" style="align-items:center"><b>Ejercicios (2)</b><div class="btn2">＋ Agregar ejercicio</div></div>'
  + card('1. Sentadilla libre', '4 × 12 · descanso 90 s', 'Piernas', 'o') + card('2. Plancha abdominal', '3 × 40 · descanso 30 s', 'Abdomen', 'o') + '<div class="btn">GUARDAR</div></div>')
P['09_asistencia'] = ('Asistencia', barra('Asistencia') + '<div class="cuerpo">' + filtros('Nombre del cliente', 'Este mes') + '<small style="color:#666">4 asistencia(s) · 1 en el gimnasio ahora</small>'
  + card('12/11/2026', 'Entrada: 12/11/2026 06:05 a. m.', 'En el gimnasio', 'v') + card('11/11/2026', 'Entrada: 06:10 a. m. · Salida: 07:10 a. m.', '1 h 0 min', 'g', 'Registrado por Andrés Cliente')
  + card('10/11/2026', 'Entrada: 06:00 a. m. · Salida: 07:15 a. m.', '1 h 15 min', 'g') + '</div><div class="fab">＋ Registrar entrada</div>'
  + '<div class="snack" style="bottom:90px">El cliente no tiene una membresía vigente. Debe adquirir o renovar un plan. (422)</div>')
P['10_progreso_form'] = ('Registro de progreso', barra('Nueva medición') + '<div class="cuerpo"><small style="color:#666">Cliente</small>' + campo('Andrés Cliente ▾') + campo('Fecha 📅  12/11/2026', 'La fecha no puede ser futura.')
  + campo('Peso (kg)  76.4') + campo('Estatura (cm, opcional)  175') + campo('% Grasa corporal (opcional)  95', 'El % de grasa debe estar entre 2 y 70.') + campo('Observaciones (opcional)')
  + '<div class="btn">GUARDAR</div><div class="nota">Al guardar, la API calcula el IMC (24.9) y lo devuelve.</div></div>')
P['11_usuarios'] = ('Gestión de usuarios', barra('Usuarios') + '<div class="cuerpo">' + filtros('Nombre, correo o documento', 'Todos los roles') + '<small style="color:#666">6 registro(s)</small>'
  + card('Administrador Golden Gym', 'admin@goldengym.com', 'Administrador', 'r') + card('Carlos Arias', 'carlos.arias@goldengym.com', 'Entrenador', 'a', 'Doc. 1000000002')
  + card('Andrés Cliente', 'andres.cliente@goldengym.com', 'Cliente', 'v', 'Tel. 3201234567') + card('Miguel Rojas', 'miguel.rojas@goldengym.com', 'Inactivo', 'g') + '</div><div class="fab">＋ Nuevo</div>')
P['12_perfil'] = ('Perfil y preferencias', barra('Perfil') + '<div class="cuerpo"><b>andres.cliente@goldengym.com</b><br><small style="color:#666">Rol: Cliente · Documento: 1000000004</small>'
  + campo('Nombres  Andrés') + campo('Apellidos  Cliente') + campo('Teléfono  32012', 'El teléfono debe tener entre 7 y 15 dígitos.') + campo('Fecha de nacimiento 📅  10/06/2001')
  + '<div class="btn">GUARDAR</div><b>Preferencias</b><div class="sw off"><span>Modo oscuro</span><span></span></div><div class="sw"><span>Recordar mi correo</span><span></span></div>'
  + '<div class="btn2">Cambiar contraseña</div><div class="btn2" style="background:#fff3c4">Cerrar sesión</div></div>')
def tel(c): return f'<div class="tel"><div class="pantalla">{c}</div></div>'
async def main():
    from playwright.async_api import async_playwright
    async with async_playwright() as p:
        b = await p.chromium.launch(); pg = await b.new_page(viewport={'width': 380, 'height': 800}, device_scale_factor=2)
        for k, (titulo, contenido) in P.items():
            await pg.set_content(f'<html><head><style>{CSS}</style></head><body><div style="padding:10px">{tel(contenido)}</div><div style="text-align:center;font-weight:bold;font-size:14px;margin-top:-2px">{html.escape(titulo)}</div></body></html>')
            el = await pg.query_selector('body'); await pg.set_viewport_size({'width': 384, 'height': 790})
            await pg.screenshot(path=f'../{k}.png', full_page=True)
        # Hoja resumen con todas las pantallas
        celdas = ''.join(f'<div style="display:inline-block;vertical-align:top;margin:10px;text-align:center">{tel(c)}<div style="font-weight:bold;margin-top:6px">{html.escape(t)}</div></div>' for t, c in P.values())
        await pg.set_viewport_size({'width': 1580, 'height': 1000})
        await pg.set_content(f'<html><head><style>{CSS}</style></head><body><h2 style="text-align:center">Wireframes · Golden Gym (App Android)</h2><div style="width:1560px;margin:auto">{celdas}</div></body></html>')
        await pg.screenshot(path='../00_todas_las_pantallas.png', full_page=True)
        await b.close()
asyncio.run(main())
