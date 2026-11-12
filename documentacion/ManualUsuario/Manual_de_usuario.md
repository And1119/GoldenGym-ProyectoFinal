# Manual de usuario · App Golden Gym

## 1. Ingresar a la app

1. Abrir **Golden Gym**.
2. Escribir el correo y la contraseña y tocar **Ingresar**. Si se marca **Recordar mi correo**, la próxima vez el correo aparecerá escrito.
3. Si no tienes cuenta, toca **¿No tienes cuenta? Regístrate**. Llena tus datos; la contraseña debe tener mínimo 8 caracteres, con una mayúscula, una minúscula y un número. Al terminar entras directamente con rol **Cliente**.
4. El ícono ⚙ permite cambiar la dirección del servidor y probar la conexión.

Si la sesión expira (después de 2 horas), la app vuelve sola al Login con el mensaje “Tu sesión expiró”.

## 2. Pantalla de inicio (Home)

Muestra tu nombre, correo y rol, unos indicadores y los módulos que tu rol puede usar. Desliza hacia abajo para actualizar.

| Rol | Indicadores | Módulos |
|---|---|---|
| Cliente | Membresía vigente y días restantes, rutinas activas, asistencias del mes, último peso | Membresías, Rutinas, Asistencia, Progreso, Ejercicios, Planes, Perfil. Además, el botón **Registrar entrada**. |
| Entrenador | Clientes activos, membresías vigentes, membresías que vencen en 3 días, asistencias de hoy, mis rutinas | Los anteriores y **Clientes** |
| Administrador | Los mismos del entrenador | Los anteriores y **Usuarios** |

En todas las listas hay una **búsqueda por texto** y un **filtro** (rol, estado, nivel, grupo muscular o periodo). El botón dorado **＋** crea un registro nuevo si tu rol lo permite.

## 3. Cliente

- **Adquirir un plan:** en Planes, toca un plan; o en Membresías, toca **Adquirir plan**. Elige la fecha de inicio. La app muestra la fecha de fin y el total. Toca **Confirmar compra**. Si ya tienes una membresía en esas fechas, la app te avisa y no deja comprar.
- **Mis membresías:** muestra el estado de cada una (Vigente, Pendiente, Vencida o Cancelada) y los días restantes.
- **Registrar asistencia:** en Home o en Asistencia, toca **Registrar entrada**. Al salir, toca el registro y elige **Registrar salida**. Solo funciona con una membresía vigente.
- **Mis rutinas:** toca una rutina para ver sus ejercicios, series, repeticiones y descanso.
- **Mi progreso:** muestra tus mediciones y cuánto ha cambiado tu peso.

## 4. Entrenador

- **Ejercicios:** crear, editar y desactivar ejercicios del catálogo.
- **Rutinas:** toca **Nuevo**, escribe el nombre, elige el nivel y el cliente, y agrega ejercicios con **Agregar ejercicio** (series, repeticiones y descanso). Toca un ejercicio de la lista para editarlo o quitarlo. Solo puedes editar las rutinas que tú creaste; si intentas editar la de otro entrenador, aparece **Acceso denegado**.
- **Asistencia:** **Registrar entrada** te pide elegir el cliente que llega.
- **Progreso:** **Nuevo** permite registrar peso, estatura, % de grasa y observaciones. La app calcula el IMC.
- **Clientes:** consulta los datos de los clientes en modo de solo lectura.

## 5. Administrador

- **Usuarios:** crear usuarios con cualquier rol, editar datos, cambiar el rol y activar o desactivar cuentas (menú 🗑 → Desactivar). No puedes desactivarte a ti mismo.
- **Planes:** crear y editar planes (nombre, duración, precio). Desactivar un plan no cambia las membresías que ya se vendieron.
- **Membresías:** asignar un plan a un cliente, cambiar el plan o la fecha de inicio y **Cancelar membresía**.
- **Asistencia:** además de registrar, puede eliminar registros equivocados.

## 6. Perfil y preferencias

- Editar nombres, apellidos, teléfono y fecha de nacimiento.
- **Modo oscuro** y **Recordar mi correo**: se guardan en el teléfono y se mantienen aunque cierres la app.
- **Cambiar contraseña:** pide la contraseña actual y la nueva.
- **Cerrar sesión:** borra el token y tus datos del teléfono y vuelve al Login.

## 7. Mensajes frecuentes

| Mensaje | Qué significa |
|---|---|
| No se pudo conectar con el servidor | La API está apagada o la URL configurada no es correcta |
| Tu sesión expiró | El token venció; vuelve a ingresar |
| Acceso denegado | Tu rol no puede hacer esa acción |
| El cliente no tiene una membresía vigente | Hay que adquirir o renovar un plan antes de entrar |
| El cliente ya tiene una membresía activa del … al … | Elige una fecha de inicio posterior a la membresía actual |
