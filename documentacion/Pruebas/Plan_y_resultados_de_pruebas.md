# Plan y resultados de pruebas

## 1. Tipos de prueba

| Tipo | Herramienta | Ubicación | Cómo se ejecuta |
|---|---|---|---|
| Unitarias del dominio (backend) | xUnit | `backend/GoldenGym.Tests/Unit` | `dotnet test` |
| Integración API + JWT + roles | xUnit + WebApplicationFactory + EF Core InMemory | `backend/GoldenGym.Tests/Integration` | `dotnet test` (no necesita XAMPP) |
| Unitarias de la app (validaciones y formatos) | JUnit 4 | `android/app/src/test` | Android Studio → `ValidadorTest` → Run, o `gradlew test` |
| Manuales API | Swagger UI / `GoldenGym.Api.http` | `http://localhost:5080/swagger` | Ver sección 4 |
| Manuales extremo a extremo (Android → API → BD) | Emulador + phpMyAdmin | — | Ver sección 5 |
| Base de datos | MySQL / MariaDB | `database/` | Ver sección 6 |

## 2. Pruebas automáticas del backend (35 casos)

Ejecutar desde la carpeta `backend`:

```bash
dotnet test
```

### 2.1 Seguridad: JWT y roles (`SeguridadApiTests`)

| # | Prueba | Resultado esperado |
|---|---|---|
| 1 | Login_ConCredencialesValidas_DevuelveTokenYRol | 200, JWT con 3 partes y rol Administrador |
| 2 | Login_ConPasswordIncorrecta_Devuelve401 | 401, “Correo o contraseña incorrectos.” |
| 3 | Login_UsuarioInactivo_Devuelve401 | 401 |
| 4 | EndpointProtegido_SinToken_Devuelve401 | 401 |
| 5 | EndpointProtegido_ConTokenAlterado_Devuelve401 | 401 (la firma no coincide) |
| 6 | Registro_CorreoDuplicado_Devuelve409 | 409 (aunque el correo venga en mayúsculas) |
| 7 | Registro_DatosInvalidos_Devuelve400ConErroresPorCampo | 400 con errores en `correo` y `password` |
| 8 | Registro_Valido_CreaClienteYDevuelveToken | 201 y rol Cliente |
| 9 | Cliente_ListarUsuarios_Devuelve403 | 403 |
| 10 | Cliente_CrearEjercicio_Devuelve403 | 403 |
| 11 | Entrenador_CrearPlan_Devuelve403 | 403 |
| 12 | Entrenador_ListarUsuarios_SoloVeClientes | 200, solo usuarios con rol Cliente |
| 13 | Admin_ListarUsuariosFiltrados_Devuelve200 | 200, 2 entrenadores activos |
| 14 | Cliente_VerRutinaDeOtroCliente_Devuelve403 | 403 |
| 15 | Entrenador_EditarRutinaDeOtroEntrenador_Devuelve403 | 403 |
| 16 | PlanesPublicos_SinToken_Devuelve200SoloActivos | 200, sin planes inactivos |

### 2.2 Reglas de negocio y CRUD (`ReglasNegocioApiTests`)

| # | Prueba | Resultado esperado |
|---|---|---|
| 17 | Ejercicio_CrudCompleto_PorEntrenador | Crear 201 → duplicado 409 → editar 200 → buscar por 2 criterios → desactivar 204 |
| 18 | Membresia_CruceDeFechas_Devuelve422 | 422 |
| 19 | Membresia_PlanInactivo_Devuelve422 | 422 |
| 20 | Membresia_ClienteCompraPlan_CalculaFechaFinYQuedaVigente | 201, fin = inicio + 6 días, 7 días restantes, Id tomado del token |
| 21 | Asistencia_SinMembresiaVigente_Devuelve422 | 422, “membresía vigente” |
| 22 | Asistencia_EntradaDobleYSalida | 201 → segunda entrada 422 → salida 200 |
| 23 | Rutina_ConEjercicioInactivoODuplicado_Devuelve422 | 422 y 422 |
| 24 | Rutina_ActualizarDetalle_SincronizaEjercicios | 200, la tabla RutinaEjercicio queda actualizada |
| 25 | Progreso_FechaFutura_Devuelve422_YCalculaImc | 422; luego 201 con IMC 20.0 |
| 26 | Admin_NoPuedeDesactivarseASiMismo | 422 |
| 27 | Dashboard_Cliente_MuestraMembresiaVigente | 200, Plan Mensual |
| 28 | RecursoInexistente_Devuelve404 | 404 |

### 2.3 Dominio (`DominioTests`)

| # | Prueba | Resultado esperado |
|---|---|---|
| 29–31 | CalcularFechaFin_IncluyeElDiaDeInicio (1, 7 y 30 días) | 12/11, 18/11 y 11/12/2026 |
| 32 | EstadoCalculado_SegunFechas | Pendiente / Vigente / Vencida / Cancelada |
| 33 | SeCruzaCon_DetectaSolapamientos | Detecta el cruce en los bordes |
| 34 | Imc_SeCalculaConUnDecimal | 76.4 kg y 175 cm → 24.9 |
| 35 | Asistencia_MinutosEnGimnasio | 95 minutos |

### 2.4 Verificación realizada durante el desarrollo

- **Lógica de negocio (capa Application):** se ejecutaron **42 escenarios** con repositorios en memoria. Resultado: **42 pasaron y 0 fallaron**. Cubren el registro, el login, los permisos por rol, las membresías (cálculo de fechas, cruces, estados, cancelación), la asistencia, las rutinas, el progreso y el dashboard.
- **Compilación:** las capas Domain y Application compilan sin errores ni advertencias. El backend completo y la app Android se revisaron contra las firmas de sus librerías sin errores de tipos.
- **Base de datos:** `schema.sql` y `seed.sql` se ejecutaron en MariaDB 10.11 y se comprobaron las restricciones (ver sección 6).

> Ejecuten `dotnet test` en su equipo y peguen aquí la captura de la consola con el resultado (esperado: 35 correctas).

## 3. Pruebas unitarias de la app (JUnit)

| # | Prueba | Qué verifica | Resultado |
|---|---|---|---|
| A1 | correo_valido_e_invalido | Formato del correo | ✅ |
| A2 | password_exige_mayuscula_minuscula_numero_y_8_caracteres | Política de contraseña | ✅ |
| A3 | nombre_minimo_dos_caracteres | Longitud del nombre | ✅ |
| A4 | campos_opcionales | Teléfono y documento opcionales | ✅ |
| A5 | rangos_numericos | Series, peso y estatura | ✅ |
| A6 | fechas | No futura / no pasada | ✅ |
| A7 | formatos_de_fecha_y_tiempo | dd/MM/yyyy, minutos, decimales | ✅ |

## 4. Pruebas manuales en Swagger

1. Ejecutar la API y abrir `http://localhost:5080/swagger`.
2. Llamar `POST /api/auth/login` con `{"correo":"andres.cliente@goldengym.com","password":"Cliente123*"}` y copiar el `token`.
3. Pulsar **Authorize**, pegar el token y pulsar **Authorize**.
4. Llamar `GET /api/usuarios` → **403** (el cliente no tiene permiso).
5. Repetir el login con `admin@goldengym.com / Admin123*` → `GET /api/usuarios?rol=Cliente&buscar=and` → **200**.
6. Pulsar **Logout** en Authorize y llamar `GET /api/dashboard` → **401**.

## 5. Pruebas manuales extremo a extremo (sustentación)

| # | Escenario | Pasos | Resultado esperado |
|---|---|---|---|
| P-01 | Login con dos roles | Ingresar como cliente y luego como administrador | El Home muestra módulos e indicadores distintos |
| P-02 | Registro con correo duplicado | Registrarse con `admin@goldengym.com` | Error bajo el campo correo (409) |
| P-03 | Alta desde Android | Entrenador → Ejercicios → Nuevo → “Remo con barra” | El ejercicio aparece en la lista y en la tabla `Ejercicios` (phpMyAdmin) |
| P-04 | Consulta y filtro | Ejercicios: escribir “press” y filtrar “Pecho” | Solo aparecen los ejercicios coincidentes |
| P-05 | Edición | Administrador → Planes → Plan Semanal → precio 42000 → Guardar | Se ve el cambio en la lista y en la tabla `Planes` |
| P-06 | Desactivar | Administrador → Usuarios → Miguel Rojas → Desactivar | Etiqueta “Inactivo”; Miguel ya no puede iniciar sesión |
| P-07 | **Acceso denegado (403)** | Entrar como `laura.mendez@goldengym.com` → Rutinas → “Fuerza tren superior” (de Carlos) → Editar → Guardar | Diálogo “Acceso denegado: Solo puedes modificar las rutinas que tú creaste.” |
| P-08 | Regla de negocio (422) | Cliente Andrés → Membresías → Adquirir plan con fecha de hoy | “El cliente ya tiene una membresía activa…” |
| P-09 | Asistencia | Cliente Andrés → Home → Registrar entrada; luego Asistencia → tocar el registro → Registrar salida | Se crea la entrada y luego se calculan los minutos |
| P-10 | Asistencia sin plan | Cliente Miguel (membresía vencida) → Asistencia → Registrar entrada | 422: “no tiene una membresía vigente” |
| P-11 | Preferencias | Perfil → activar Modo oscuro; cerrar y abrir la app | El tema oscuro se conserva |
| P-12 | Cerrar sesión | Home → ⋮ → Cerrar sesión; luego pulsar Atrás | Se abre el Login y no se puede volver al Home |
| P-13 | Sin conexión | Detener la API y tocar Ingresar | “No se pudo conectar con el servidor…” sin que la app se cierre |
| P-14 | Rendimiento | Swagger: `GET /api/membresias` como administrador | Menos de 3 s (ver “Duration” en Swagger) |
| P-15 | Integridad SQL | phpMyAdmin: `DELETE FROM Ejercicios WHERE IdEjercicio = 1;` | Error 1451 (FK RESTRICT): el ejercicio está en una rutina |
| P-16 | Token expirado | En `appsettings.json` poner `"ExpiracionMinutos": 1`, iniciar sesión y esperar 2 minutos | La app vuelve al Login con “Tu sesión expiró” |

## 6. Pruebas de la base de datos (ejecutadas en MariaDB 10.11)

| Prueba | Sentencia | Resultado obtenido |
|---|---|---|
| Crear el esquema y los datos | `mysql -u root < schema.sql` y luego `< seed.sql` | 9 tablas; 6 usuarios (1 administrador, 2 entrenadores, 3 clientes) |
| Correo único | `INSERT` con `admin@goldengym.com` | `ERROR 1062 Duplicate entry … UQ_Usuarios_Correo` ✅ |
| Integridad referencial | `DELETE FROM Ejercicios WHERE IdEjercicio=1` | `ERROR 1451 … FK_RutinaEjercicio_Ejercicios` ✅ |
| Restricción CHECK | `INSERT INTO Progresos … PesoKg = 5` | `ERROR 4025 CONSTRAINT CK_Progresos_Peso failed` ✅ |
| Hash de contraseñas | Verificar BCrypt de `Admin123*` | `True` ✅ |
