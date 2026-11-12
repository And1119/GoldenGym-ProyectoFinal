# Documento empresarial y planteamiento del problema

**Proyecto final integrador · Desarrollo de Aplicaciones Móviles Nativas · UCompensar**
**Grupo 9 · Aplicación GYM**

| Campo | Detalle |
|---|---|
| Empresa desarrolladora (ficticia) | FitCore Technologies S.A.S. |
| Cliente / negocio | Golden Gym Ibagué (Golden Center Gym) |
| Ubicación | Calle 25 # 4C-11, Barrio El Carmen, piso 3, Ibagué (Tolima) |
| Producto | App Android “Golden Gym” + API REST ASP.NET Core + base de datos MySQL |
| Antecedente | Página web informativa del gimnasio (HTML/CSS/JS + Node.js) con planes, horarios, carrito y formulario de registro |

## 1. Contexto empresarial

Golden Gym Ibagué es un gimnasio de barrio que ofrece área de pesas, máquinas de musculación, entrenamiento funcional, zona cardiovascular, clases de cardio-rumba y atención semi-personalizada. Atiende de lunes a viernes de 6:00 a.m. a 12:00 m. y de 2:00 p.m. a 9:30 p.m., y los sábados y festivos de 7:00 a.m. a 1:30 p.m. Los domingos no presta servicio.

Hoy vende cuatro planes:

| Plan | Duración | Precio |
|---|---|---|
| Rutina por sesión | 1 día | $15.000 |
| Plan semanal | 7 días | $40.000 |
| Plan quincenal | 15 días | $55.000 |
| Plan mensual | 30 días | $65.000 |

El gimnasio ya tiene una página web que muestra esta información y recibe formularios de registro y compras simuladas. Sin embargo, esa página **no guarda la información**: los datos llegan al servidor y solo se imprimen en consola.

## 2. Problema

El gimnasio necesita gestionar afiliados, membresías, rutinas y asistencia, pero la información está dispersa (cuadernos, chats y la memoria de los entrenadores). Esto provoca:

1. **No se sabe con certeza quién tiene la membresía vigente.** Se dejan entrar clientes con el plan vencido o se cobran membresías que se cruzan en fechas.
2. **Las rutinas se entregan en papel o por WhatsApp.** No hay registro de qué entrenador asignó qué rutina ni de los cambios.
3. **No hay control de asistencia.** No se sabe cuántas personas entrenan por día ni cuánto tiempo permanecen.
4. **El progreso físico (peso, % de grasa, IMC) no tiene seguimiento histórico.**
5. **No hay control de acceso.** Cualquier persona con el cuaderno puede modificar la información.

## 3. Objetivo general

Implementar una solución Android conectada a una API REST que permita gestionar afiliados, membresías, rutinas, ejercicios, asistencia y progreso del gimnasio Golden Gym, con autenticación JWT y permisos por rol.

## 4. Objetivos específicos

- Registrar clientes con correo único e iniciar sesión mediante el backend con JWT.
- Controlar la vigencia de las membresías e impedir membresías con fechas cruzadas.
- Permitir que los entrenadores creen rutinas con ejercicios, series y repeticiones, y las asignen a cada cliente.
- Registrar la entrada y salida de los clientes, validando que tengan membresía vigente.
- Llevar el historial de mediciones físicas y calcular el IMC.
- Restringir cada funcionalidad según el rol (Administrador, Entrenador o Cliente).

## 5. Alcance

**Incluye:**

- App Android en Kotlin con Activities y XML.
- API REST en C# .NET 8 con arquitectura por capas (Domain, Application, Infrastructure, Api).
- Base de datos MySQL/MariaDB (XAMPP) con 9 tablas relacionadas.
- Swagger, pruebas automáticas, documentación y manuales.

**No incluye:**

- Pagos reales. La compra de planes es simulada y solo registra la membresía con el precio del plan.
- Notificaciones push.
- Publicación en Play Store.

## 6. Actores y permisos

| Actor | Descripción | Permisos principales |
|---|---|---|
| Visitante | Persona sin cuenta | Registrarse como cliente y consultar los planes |
| Cliente | Afiliado del gimnasio | Ver su Home; adquirir un plan; consultar sus membresías, rutinas, asistencias y progreso; registrar su entrada y salida; editar su perfil |
| Entrenador | Personal del gimnasio | Todo lo del usuario autenticado, más gestionar ejercicios y rutinas (solo editar las propias), registrar la asistencia de clientes, registrar progreso y consultar clientes |
| Administrador | Dueño o gerente | Todo lo del entrenador, más gestionar usuarios y roles, planes y membresías (crear, editar y cancelar), y eliminar registros de asistencia |

## 7. Reglas de negocio

| ID | Regla | Dónde se valida |
|---|---|---|
| RN-01 | El correo es único por usuario y se guarda en minúsculas. El documento, si se registra, también es único. | API (409) + BD (UNIQUE) |
| RN-02 | El registro público siempre crea usuarios con rol **Cliente**. Solo el administrador crea entrenadores o administradores. | API |
| RN-03 | Las contraseñas tienen mínimo 8 caracteres, con mayúscula, minúscula y número, y se guardan con hash **BCrypt**. | App + API |
| RN-04 | La fecha de fin de una membresía es la fecha de inicio más la duración del plan, menos 1 día. El precio pagado se copia del plan al momento de la venta. | API |
| RN-05 | Un cliente no puede tener dos membresías **activas** con fechas que se crucen. | API (422) |
| RN-06 | No se venden planes inactivos y una membresía nueva no puede iniciar antes de hoy ni más de 60 días después. | API (422) |
| RN-07 | Las membresías no se borran, se **cancelan**. El estado se calcula así: Vigente, Pendiente (aún no inicia), Vencida o Cancelada. | API |
| RN-08 | Solo se registra la entrada de un cliente activo con **membresía vigente**, y no puede haber dos entradas abiertas el mismo día. | API (422) |
| RN-09 | Una rutina pertenece a un cliente, tiene al menos un ejercicio, no repite ejercicios y solo usa ejercicios activos. | App + API |
| RN-10 | Un entrenador solo puede modificar o desactivar las rutinas que él creó. El administrador puede modificar todas. | API (403) |
| RN-11 | El progreso no puede tener fecha futura. Peso entre 20 y 400 kg, estatura entre 50 y 250 cm, grasa entre 2 y 70 %. | App + API + BD (CHECK) |
| RN-12 | Usuarios, planes, ejercicios y rutinas se **desactivan** en lugar de borrarse para conservar el historial (integridad referencial). | API + BD (FK RESTRICT) |
| RN-13 | Un administrador no puede desactivarse ni quitarse su propio rol. | API (422) |
| RN-14 | El cliente solo ve su propia información aunque envíe el Id de otro usuario. La API toma el Id del token. | API (403) |

## 8. Respuestas a las preguntas orientadoras

**¿Quiénes son los actores y qué permisos tiene cada uno?**
Los cuatro actores de la sección 6. Los permisos se implementan con políticas JWT (`SoloAdmin`, `Personal`, `Autenticado`) y con validaciones de propiedad en los servicios.

**¿Qué datos son obligatorios y cuáles deben ser únicos?**
Son obligatorios los nombres, apellidos, correo y contraseña del usuario; plan y fecha de inicio de la membresía; nombre, nivel, cliente y al menos un ejercicio de la rutina; y fecha y peso del progreso. Deben ser únicos el correo, el documento, el nombre del plan, el nombre del ejercicio y el par (rutina, ejercicio).

**¿Qué reglas de negocio impiden operaciones inválidas?**
Las reglas RN-01 a RN-14. La API responde con 409 (duplicado), 422 (regla de negocio) o 403 (sin permiso).

**¿Qué ocurre cuando no hay conexión o expira el JWT?**
Sin conexión, el repositorio captura la excepción de red y la app muestra “No se pudo conectar con el servidor” sin cerrarse. Si el JWT expira (2 horas), la API responde 401 y la app borra la sesión local y vuelve al Login con el mensaje “Tu sesión expiró”. Además, al abrir la app se revisa la fecha de expiración guardada.

**¿Qué información necesita persistencia local y cuál va en servidor?**
- **Local:** el token JWT, el rol, el nombre y el correo del usuario (cifrados con EncryptedSharedPreferences), y las preferencias de interfaz (modo oscuro, recordar correo, URL del servidor).
- **Servidor:** toda la información del negocio en MySQL.

**¿Cómo se garantiza la integridad y la trazabilidad?**
- **Integridad:** claves primarias y foráneas con `ON DELETE RESTRICT`, restricciones `UNIQUE` y `CHECK` en la base de datos, validaciones en la app y en la API, y borrado lógico.
- **Trazabilidad:** cada membresía guarda la fecha de registro y el precio pagado, cada rutina guarda su entrenador y su fecha de creación, cada asistencia guarda quién la registró y cada progreso guarda el entrenador que lo evaluó.
