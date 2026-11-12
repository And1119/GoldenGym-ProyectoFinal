# Matriz de requisitos funcionales y no funcionales

Cada requisito se relaciona con el endpoint de la API, la pantalla Android, el caso de uso y la prueba que lo verifica.

## Requisitos funcionales transversales (enunciado del docente)

| ID | Requisito | Endpoint API | Pantalla Android | Caso de uso | Prueba |
|---|---|---|---|---|---|
| RF-01 | Registro de usuarios y validación de correo único | `POST /api/auth/registro` | RegistroActivity | UC-01 | `Registro_CorreoDuplicado_Devuelve409`, `Registro_Valido_CreaCliente…` |
| RF-02 | Login mediante backend con JWT | `POST /api/auth/login` | LoginActivity | UC-02 | `Login_ConCredencialesValidas_DevuelveTokenYRol`, `Login_ConPasswordIncorrecta_Devuelve401` |
| RF-03 | Autorización por al menos dos roles (aquí son tres) | Políticas `SoloAdmin`, `Personal`, `Autenticado` | Home (módulos según el rol) | Todos | `Cliente_ListarUsuarios_Devuelve403`, `Entrenador_CrearPlan_Devuelve403` |
| RF-04 | Home con perfil autenticado | `GET /api/dashboard`, `GET /api/auth/perfil` | HomeActivity, PerfilActivity | UC-03 | `Dashboard_Cliente_MuestraMembresiaVigente` |
| RF-05 | Crear información del negocio | `POST` en planes, membresías, ejercicios, rutinas, asistencias y progresos | Formularios (*FormActivity) | UC-07 a UC-17 | `Ejercicio_CrudCompleto_PorEntrenador` |
| RF-06 | Listar registros por API | `GET /api/{recurso}` | ListaBaseActivity (7 listados) | UC-11, 14, 16, 18, 19 | `Admin_ListarUsuariosFiltrados_Devuelve200` |
| RF-07 | Editar registros | `PUT /api/{recurso}/{id}` | Formularios en modo edición | UC-07 a UC-17 | `Rutina_ActualizarDetalle_SincronizaEjercicios` |
| RF-08 | Eliminar o desactivar registros | `DELETE /api/{recurso}/{id}` (borrado lógico) | Menú “Desactivar” / “Cancelar membresía” | UC-07, 08, 09, 12, 13 | `Ejercicio_CrudCompleto…` (paso desactivar) |
| RF-09 | Buscar y filtrar por dos criterios | Parámetros `buscar` + `rol` / `estado` / `nivel` / `grupoMuscular` / `desde` | Barra de búsqueda + filtro desplegable en cada lista | — | `Ejercicio_CrudCompleto…` (`?buscar=Remo&grupoMuscular=Espalda`) |
| RF-10 | Consultar detalle | `GET /api/{recurso}/{id}` | RutinaDetalleActivity, formularios en modo detalle | UC-14 | `Cliente_VerRutinaDeOtroCliente_Devuelve403` |
| RF-11 | Guardar preferencias de interfaz | — (local) | PerfilActivity (modo oscuro, recordar correo), Login (URL del servidor) | UC-05 | Prueba manual P-11 |
| RF-12 | Cerrar sesión y limpiar credenciales locales | — (local) | Home → menú “Cerrar sesión”, Perfil | UC-06 | Prueba manual P-12 |

## Requisitos funcionales específicos del Grupo 9 (Aplicación GYM)

| ID | Requisito | Endpoint API | Pantalla | Regla relacionada |
|---|---|---|---|---|
| RF-13 | Gestionar membresías (asignar, editar, cancelar, consultar estado) | `/api/membresias` | MembresiasActivity, MembresiaFormActivity | RN-04, RN-05, RN-06, RN-07 |
| RF-14 | Adquirir un plan desde la app (compra simulada) | `POST /api/membresias` (rol Cliente) | PlanesActivity → MembresiaFormActivity | RN-05, RN-14 |
| RF-15 | Gestionar el catálogo de planes | `/api/planes` | PlanesActivity, PlanFormActivity | RN-12 |
| RF-16 | Gestionar ejercicios | `/api/ejercicios` | EjerciciosActivity, EjercicioFormActivity | RN-12 |
| RF-17 | Gestionar rutinas con su detalle de ejercicios (tabla RutinaEjercicio) | `/api/rutinas` | RutinasActivity, RutinaDetalleActivity, RutinaFormActivity | RN-09, RN-10 |
| RF-18 | Registrar asistencia (entrada / salida) | `POST /api/asistencias/entrada`, `PUT /api/asistencias/{id}/salida` | AsistenciaActivity, botón rápido en Home | RN-08 |
| RF-19 | Registrar y consultar el progreso físico con cálculo de IMC | `/api/progresos` | ProgresoActivity, ProgresoFormActivity | RN-11 |
| RF-20 | Gestionar perfiles y usuarios por rol | `/api/usuarios`, `/api/auth/perfil` | UsuariosActivity, UsuarioFormActivity, PerfilActivity | RN-02, RN-13 |

## Requisitos no funcionales

| ID | Requisito | Cómo se cumple | Evidencia |
|---|---|---|---|
| RNF-01 | Usabilidad: formularios etiquetados y mensajes claros | Todos los campos usan `TextInputLayout` con *hint*, texto de ayuda y error bajo el campo. Los mensajes de la API vienen en español. | Wireframes, capturas |
| RNF-02 | Rendimiento: objetivo de 3 s en consultas habituales | Consultas con índices (`IX_Membresias_Usuario_Fechas`, `IX_Asistencias_Usuario_Fecha`, `IX_Progresos_Usuario_Fecha`), *timeouts* de 10 a 15 s en OkHttp y búsqueda con espera de 400 ms para no saturar la API | Prueba P-14 (Swagger) |
| RNF-03 | Seguridad: JWT, roles y validación en servidor | JWT HS256 que valida emisor, audiencia, firma y expiración (120 min); políticas por rol; BCrypt (factor 11); el token se guarda cifrado (EncryptedSharedPreferences AES-256); el token se oculta en los logs | Pruebas 401/403 |
| RNF-04 | Persistencia: datos conservados entre sesiones | MySQL/MariaDB para el negocio. La sesión y las preferencias quedan en el dispositivo. | `schema.sql`, `seed.sql` |
| RNF-05 | Mantenibilidad: responsabilidades separadas | Backend en 4 capas (Domain, Application, Infrastructure, Api) con puertos e interfaces. Android dividido en activities, repository, api, models, preferences y utils. | Diagrama de componentes |
| RNF-06 | Compatibilidad: emulador definido en clase | minSdk 26 (Android 8.0), targetSdk 34, compileSdk 35. URL `10.0.2.2:5080` para el emulador y URL configurable para un celular físico. | `build.gradle.kts` |
| RNF-07 | Validaciones en cliente y servidor | `Validador.kt` (app) y DataAnnotations y reglas en los servicios (API) | `ValidadorTest.kt`, pruebas de la API |
| RNF-08 | Errores HTTP y de conexión controlados | Middleware que responde un JSON único `{status, mensaje, errores}`. En la app, `Resultado.Error` maneja 401 (vuelve al Login), 403 (diálogo) y sin conexión (mensaje). | Diagrama de secuencia |
| RNF-09 | Integridad referencial en SQL | FK con RESTRICT, CASCADE y SET NULL, UNIQUE, CHECK y borrado lógico | `schema.sql`, prueba P-15 |
| RNF-10 | Git, documentación y pruebas reproducibles | Repositorio Git con `.gitignore`, README, manuales, 35 pruebas automáticas de la API (xUnit) y 7 pruebas de la app (JUnit) | Carpeta `documentacion/Pruebas` |
