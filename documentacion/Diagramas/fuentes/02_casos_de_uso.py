# Genera el diagrama de casos de uso (SVG con posiciones fijas) y lo exporta a PNG con Playwright.
import asyncio, pathlib
RX, RY = 145, 25
uc = {  # id: (texto, cx, cy)
 'UC01': ('UC-01 Registrarse', 540, 135), 'UC02': ('UC-02 Iniciar sesión (JWT)', 540, 185),
 'UC03': ('UC-03 Ver Home / perfil', 540, 235), 'UC04': ('UC-04 Editar perfil y contraseña', 540, 285),
 'UC05': ('UC-05 Configurar preferencias', 540, 335), 'UC06': ('UC-06 Cerrar sesión', 540, 385),
 'UC10': ('UC-10 Adquirir plan', 540, 480), 'UC11': ('UC-11 Consultar mis membresías', 540, 530),
 'UC14': ('UC-14 Consultar mis rutinas', 540, 580), 'UC18': ('UC-18 Consultar mi progreso', 540, 630),
 'UC07': ('UC-07 Gestionar usuarios y roles', 1060, 135), 'UC08': ('UC-08 Gestionar planes', 1060, 185),
 'UC09': ('UC-09 Gestionar membresías', 1060, 235),
 'UC12': ('UC-12 Gestionar ejercicios', 1060, 480), 'UC13': ('UC-13 Gestionar rutinas', 1060, 530),
 'UC17': ('UC-17 Registrar / editar progreso', 1060, 580), 'UC19': ('UC-19 Consultar clientes', 1060, 630),
 'UC15': ('UC-15 Registrar entrada / salida', 800, 760), 'UC16': ('UC-16 Consultar asistencias', 800, 830),
 'INC1': ('Validar token JWT y rol', 800, 300), 'INC2': ('Validar membresía vigente', 800, 930),
}
actores = {'Visitante': (150, 135), 'Usuario autenticado': (150, 285), 'Cliente': (150, 600),
           'Administrador': (1450, 185), 'Entrenador': (1450, 560)}
grupos = [('Cuenta y sesión', 385, 102, 310, 313), ('Autoservicio del cliente', 385, 447, 310, 213),
          ('Administración', 905, 102, 310, 163), ('Entrenamiento', 905, 447, 310, 213), ('Asistencia', 645, 727, 310, 133)]
s = []
s.append('<rect x="330" y="50" width="940" height="935" rx="18" fill="#FFFDF5" stroke="#B8941F" stroke-width="3"/>')
s.append('<text x="800" y="80" text-anchor="middle" font-size="20" font-weight="bold" fill="#1A1A1A">Sistema Golden Gym (App Android + API REST)</text>')
for t, x, y, w, h in grupos:
    s.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="12" fill="none" stroke="#C8C8C8" stroke-dasharray="6 4"/>')
    s.append(f'<text x="{x+8}" y="{y-6}" font-size="13" font-style="italic" fill="#8A7A40">{t}</text>')
def actor(nombre, x, y):
    return (f'<g stroke="#222" stroke-width="3" fill="none"><circle cx="{x}" cy="{y-38}" r="13"/><line x1="{x}" y1="{y-25}" x2="{x}" y2="{y+10}"/>'
            f'<line x1="{x-22}" y1="{y-12}" x2="{x+22}" y2="{y-12}"/><line x1="{x}" y1="{y+10}" x2="{x-18}" y2="{y+40}"/>'
            f'<line x1="{x}" y1="{y+10}" x2="{x+18}" y2="{y+40}"/></g>'
            f'<text x="{x}" y="{y+62}" text-anchor="middle" font-size="16" font-weight="bold">{nombre}</text>')
def linea(x1, y1, x2, y2): s.append(f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="#555" stroke-width="1.6"/>')
izq = lambda k: (uc[k][1] - RX, uc[k][2]); der = lambda k: (uc[k][1] + RX, uc[k][2])
asoc = {'Visitante': ['UC01'], 'Usuario autenticado': ['UC02', 'UC03', 'UC04', 'UC05', 'UC06'],
        'Cliente': ['UC10', 'UC11', 'UC14', 'UC18', 'UC15', 'UC16'],
        'Administrador': ['UC07', 'UC08', 'UC09'], 'Entrenador': ['UC12', 'UC13', 'UC17', 'UC19', 'UC15', 'UC16']}
for a, lista in asoc.items():
    ax, ay = actores[a]
    for k in lista:
        if ax < 800: linea(ax + 30, ay - 10, *izq(k))
        else: linea(ax - 30, ay - 10, *der(k))
# include / extend
def flecha(path, etiqueta, lx, ly):
    s.append(f'<path d="{path}" fill="none" stroke="#888" stroke-width="1.5" stroke-dasharray="7 5" marker-end="url(#abierta)"/>')
    s.append(f'<text x="{lx}" y="{ly}" font-size="12" fill="#777" text-anchor="middle">{etiqueta}</text>')
flecha('M 920 145 Q 850 200 815 276', '«include»', 905, 205)
flecha('M 920 520 Q 850 420 815 326', '«include»', 905, 430)
flecha('M 935 775 Q 1010 850 903 925', '«include»', 1005, 860)
flecha('M 685 478 L 917 245', '«extend»', 830, 345)
for k, (t, x, y) in uc.items():
    gris = k.startswith('INC')
    s.append(f'<ellipse cx="{x}" cy="{y}" rx="{RX if not gris else 100}" ry="{RY}" fill="{"#EEEEEE" if gris else "#FFF2C2"}" stroke="{"#888" if gris else "#B8941F"}" stroke-width="1.6"/>')
    s.append(f'<text x="{x}" y="{y+5}" text-anchor="middle" font-size="14">{t}</text>')
for n, (x, y) in actores.items(): s.append(actor(n, x, y))
# Generalizaciones (flecha con triángulo hueco)
s.append('<path d="M 150 535 L 150 360" stroke="#222" stroke-width="2" fill="none" marker-end="url(#triangulo)"/>')
s.append('<path d="M 1450 250 L 1450 485" stroke="#222" stroke-width="2" fill="none" marker-end="url(#triangulo)"/>')
s.append('<path d="M 1450 625 L 1450 1020 L 60 1020 L 60 260 L 112 260" stroke="#222" stroke-width="2" fill="none" marker-end="url(#triangulo)"/>')
s.append('<text x="760" y="1045" text-anchor="middle" font-size="13" fill="#555">Generalización: Cliente y Entrenador son Usuarios autenticados; el Administrador hereda todo lo del Entrenador</text>')
svg = f'''<svg xmlns="http://www.w3.org/2000/svg" width="1600" height="1070" font-family="Arial" viewBox="0 0 1600 1070">
<defs><marker id="abierta" markerWidth="12" markerHeight="12" refX="10" refY="6" orient="auto"><path d="M0,0 L10,6 L0,12" fill="none" stroke="#888" stroke-width="1.5"/></marker>
<marker id="triangulo" markerWidth="18" markerHeight="18" refX="15" refY="9" orient="auto"><path d="M0,0 L16,9 L0,18 Z" fill="white" stroke="#222" stroke-width="1.6"/></marker></defs>
<rect width="1600" height="1070" fill="white"/>{"".join(s)}</svg>'''
pathlib.Path('02_casos_de_uso.svg').write_text(svg, encoding='utf-8')
async def main():
    from playwright.async_api import async_playwright
    async with async_playwright() as p:
        b = await p.chromium.launch(); pg = await b.new_page(viewport={'width': 1600, 'height': 1070}, device_scale_factor=2)
        await pg.set_content(f'<html><body style="margin:0">{svg}</body></html>'); await pg.screenshot(path='../02_casos_de_uso.png'); await b.close()
asyncio.run(main())
