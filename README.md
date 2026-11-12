# Golden Gym Ibagué · Proyecto Final Integrador

**UCompensar · Desarrollo de Aplicaciones Móviles Nativas · Grupo 9 (Aplicación GYM)**
Empresa desarrolladora (ficticia): **FitCore Technologies S.A.S.** · Cliente: **Golden Gym Ibagué**

Solución empresarial para gestionar afiliados, membresías, rutinas, ejercicios, asistencia y progreso del gimnasio. Tiene tres partes:

```
App Android (Kotlin + XML + Retrofit)  ──HTTP/JSON + JWT──▶  API REST ASP.NET Core 8  ──EF Core──▶  MySQL / MariaDB (XAMPP)
```

## Estructura de entrega

```
ProyectoFinal/
├── android/                  App Android (Android Studio)
│   └── app/src/main/java/com/goldengym/app/
│       ├── activities/       18 Activities (Login, Registro, Home, listas y formularios)
│       ├── adapters/         Adaptadores de RecyclerView
│       ├── api/              GymApi (Retrofit) y ApiClient (OkHttp + AuthInterceptor)
│       ├── models/           DTOs del JSON
│       ├── preferences/      SessionManager (token cifrado) y PreferenciasUI
│       ├── repository/       BaseRepository, AuthRepository, GymRepository, Resultado
│       └── utils/            Validador y Formatos
│       (layouts en app/src/main/res/layout)
├── backend/                  Solución .NET 8 (GoldenGym.sln)
│   ├── GoldenGym.Api/            Controllers, middleware de errores, JWT, Swagger
│   ├── GoldenGym.Application/    DTOs, interfaces (puertos) y servicios (reglas de negocio)
│   ├── GoldenGym.Domain/         Entidades y excepciones de dominio
│   ├── GoldenGym.Infrastructure/ EF Core (Pomelo MySQL), repositorios, BCrypt y JwtTokenService
│   └── GoldenGym.Tests/          35 pruebas xUnit (unitarias + integración JWT/roles)
├── database/
│   ├── schema.sql            9 tablas con PK, FK, UNIQUE, CHECK e índices
│   └── seed.sql              Roles, usuarios de prueba, planes reales del gimnasio y datos de demo
└── documentacion/
    ├── Requisitos/           Documento empresarial, reglas de negocio y matriz RF/RNF
    ├── Diagramas/            9 diagramas (PNG + fuentes)
    ├── Wireframes/           12 pantallas
    ├── Pruebas/              Plan y resultados de pruebas
    └── ManualUsuario/        Manual de instalación y manual de usuario
```

## Inicio rápido

1. **BD:** iniciar MySQL en XAMPP e importar `database/schema.sql` y luego `database/seed.sql`.
2. **API:** abrir `backend/GoldenGym.sln` y ejecutar `GoldenGym.Api`. Se abre `http://localhost:5080/swagger`.
3. **App:** abrir la carpeta `android` en Android Studio y ejecutarla en el emulador. La URL de la API es `http://10.0.2.2:5080/`.
4. Ingresar con `admin@goldengym.com / Admin123*` o `andres.cliente@goldengym.com / Cliente123*`.

Los detalles están en `documentacion/ManualUsuario/Manual_de_instalacion.md`.

## Tecnologías

| Capa | Tecnologías |
|---|---|
| Android | Kotlin 2.0, Activities + XML, ViewBinding, Material 3, Retrofit 2.11, OkHttp 4, Coroutines, EncryptedSharedPreferences |
| Backend | C# .NET 8, ASP.NET Core Web API, Swagger/OpenAPI, DTOs, inyección de dependencias, JWT Bearer, BCrypt, arquitectura por capas (hexagonal) |
| Persistencia | EF Core 8 + Pomelo MySQL, MySQL/MariaDB (XAMPP) |
| Pruebas | xUnit, WebApplicationFactory, EF InMemory, JUnit |

## Guion de sustentación (aprox. 10 minutos)

1. **Login con dos roles:** entrar como cliente (Andrés) y luego como administrador. Mostrar que el Home y los módulos cambian.
2. **Alta y consulta desde Android:** con el entrenador Carlos, crear el ejercicio “Remo con barra” y buscarlo con el filtro “Espalda”. Mostrar la fila nueva en phpMyAdmin (persistencia).
3. **Edición:** con el administrador, cambiar el precio del Plan Semanal y verlo en la tabla `Planes`.
4. **Acceso denegado (403):** con la entrenadora Laura, editar la rutina “Fuerza tren superior” (de Carlos). Aparece el diálogo “Acceso denegado”. En Swagger, mostrar `GET /api/usuarios` con el token de un cliente → 403.
5. **Manejo de errores:** comprar un plan con fechas cruzadas (422); registrar la entrada de Miguel, que tiene la membresía vencida (422); apagar la API y tocar Ingresar (sin conexión).
6. **Flujo de secuencia completo:** explicar `05_secuencia_membresia.png`: MembresiaFormActivity → GymRepository → Retrofit/AuthInterceptor → MembresiasController → MembresiaService → Repositorio/EF Core → MySQL, y la respuesta de vuelta.
7. **Pruebas:** ejecutar `dotnet test` (35 pruebas).
