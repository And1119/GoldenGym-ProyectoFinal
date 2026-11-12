# Manual de instalación

## Requisitos

| Software | Versión | Para qué |
|---|---|---|
| XAMPP | 8.x (MariaDB 10.4 o superior) | Base de datos MySQL y phpMyAdmin |
| .NET SDK | 8.0 | Compilar y ejecutar la API |
| Visual Studio 2022 (o VS Code + C# Dev Kit) | 17.8 o superior | Abrir `backend/GoldenGym.sln` |
| Android Studio | Ladybug (2024.2) o superior | Abrir la carpeta `android` |
| Emulador | Pixel con API 34 (Android 14) | Ejecutar la app |
| Git | Cualquiera | Control de versiones |

## Paso 1. Base de datos (XAMPP)

1. Abrir el **XAMPP Control Panel** y pulsar **Start** en **MySQL** (y en Apache si se va a usar phpMyAdmin).
2. Abrir `http://localhost/phpmyadmin`, ir a **Importar**, elegir `database/schema.sql` y pulsar **Continuar**.
3. Importar `database/seed.sql` de la misma forma.
4. Comprobar que existe la base `goldengym_db` con 9 tablas.

Por consola (desde `C:\xampp\mysql\bin`):

```bash
mysql -u root < ruta\ProyectoFinal\database\schema.sql
mysql -u root < ruta\ProyectoFinal\database\seed.sql
```

> Si el usuario `root` de XAMPP tiene contraseña, escríbanla en `Password=` de la cadena de conexión del paso 2.

## Paso 2. API .NET

1. Abrir `backend/GoldenGym.sln` en Visual Studio. Los paquetes NuGet se restauran solos. Si no ocurre, clic derecho en la solución → **Restaurar paquetes NuGet**.
2. Revisar `GoldenGym.Api/appsettings.json`:

```json
"ConnectionStrings": { "GoldenGymDb": "Server=localhost;Port=3306;Database=goldengym_db;User=root;Password=;" },
"Database": { "ServerVersion": "10.4.32-mariadb" }
```

   - En `ServerVersion` va la versión que muestra phpMyAdmin (por ejemplo `10.4.32-mariadb`). Si usan MySQL 8, pongan `8.0.36-mysql`.
   - Cambien `Jwt:Key` por una clave propia de al menos 32 caracteres.
3. Elegir el perfil **GoldenGym.Api** y pulsar ▶. Se abre `http://localhost:5080/swagger`.
4. Para ejecutarla por consola:

```bash
cd backend
dotnet run --project GoldenGym.Api
```

5. Para ejecutar las pruebas automáticas (no necesitan XAMPP):

```bash
cd backend
dotnet test
```

## Paso 3. App Android

1. En Android Studio: **File → Open** y elegir la carpeta `android`. Esperar a que termine el *Gradle Sync*.
2. Crear o iniciar un emulador (Pixel, API 34).
3. Pulsar **Run ▶** con el módulo `app`.
4. La app usa por defecto `http://10.0.2.2:5080/`, que es el *localhost* del PC visto desde el emulador.

### Usar un celular físico

1. El PC y el celular deben estar en la misma red Wi-Fi.
2. Ejecutar la API con el perfil **“GoldenGym.Api (red local / celular físico)”**, que escucha en `0.0.0.0:5080`.
3. Ver la IP del PC con `ipconfig` (por ejemplo `192.168.1.10`).
4. En la app, en el Login, tocar el engranaje ⚙ y escribir `http://192.168.1.10:5080/`. Luego tocar **Probar conexión**.
5. Si no conecta, permitir el puerto 5080 en el Firewall de Windows.

## Usuarios de prueba

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | admin@goldengym.com | Admin123* |
| Entrenador | carlos.arias@goldengym.com | Entrenador123* |
| Entrenador | laura.mendez@goldengym.com | Entrenador123* |
| Cliente (membresía vigente) | andres.cliente@goldengym.com | Cliente123* |
| Cliente (membresía vigente) | johana.saavedra@goldengym.com | Cliente123* |
| Cliente (membresía vencida) | miguel.rojas@goldengym.com | Cliente123* |

## Problemas frecuentes

| Problema | Solución |
|---|---|
| `Unable to connect to any of the specified MySQL hosts` | MySQL no está iniciado en XAMPP o el puerto no es 3306 |
| `Access denied for user 'root'` | Escribir la contraseña de root en la cadena de conexión |
| La app dice “No se pudo conectar con el servidor” | La API no está corriendo o la URL (⚙) es incorrecta. En el emulador debe ser `10.0.2.2`, no `localhost`. |
| `CLEARTEXT communication not permitted` | Ejecutar la variante **debug**. La versión release solo permite HTTP hacia el emulador. |
| “Configura Jwt:Key…” al iniciar la API | La clave JWT tiene menos de 32 caracteres |
| Gradle no sincroniza | Revisar la conexión a internet y que Android Studio use JDK 17 (Settings → Build Tools → Gradle → Gradle JDK) |
