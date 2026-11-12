# Diagramas del proyecto

Los archivos fuente están en `fuentes/`: Mermaid (`.mmd`) y un script de Python para el diagrama de casos de uso. Para regenerar un PNG:

```bash
mmdc -i fuentes/06_entidad_relacion.mmd -o 06_entidad_relacion.png -c fuentes/config.json -b white -s 2
```

| # | Diagrama | Archivo | Qué muestra (tarea del docente) |
|---|---|---|---|
| 1 | Arquitectura general | `01_arquitectura.png` | Protocolos (HTTP/JSON, EF Core/TCP 3306), autenticación (Bearer JWT, BCrypt, EncryptedSharedPreferences) y responsabilidades de cada capa |
| 2 | Casos de uso UML | `02_casos_de_uso.png` | 5 actores con generalización, 19 casos de uso agrupados por paquete, «include» y «extend» |
| 3 | Actividades – Login | `03_actividad_login.png` | Proceso con decisiones: sesión guardada, validación, sin conexión, 401, recordar correo y rol |
| 4 | Actividades – Asistencia | `04_actividad_asistencia.png` | Proceso con decisiones: rol, token, cliente activo, membresía vigente y entrada abierta |
| 5 | Secuencia Android–API–BD | `05_secuencia_membresia.png` | Activity → Repository → Retrofit → Controller → Service → Repositorio → BD, con caminos alternativos para fecha inválida, 401, 422 y sin conexión |
| 6 | Modelo entidad-relación | `06_entidad_relacion.png` | 9 tablas reales con PK, FK, UK, CHECK y cardinalidades |
| 7 | Componentes | `07_componentes.png` | UI, datos y utilidades (Android); Api, Application, Domain e Infrastructure (backend); persistencia |
| 8 | Navegación de pantallas | `08_navegacion.png` | Las 18 Activities y sus rutas, incluidas las restricciones por rol |
| 9 | Clases UML | `09_clases.png` | Entidades con atributos y métodos, multiplicidades, servicios e interfaces (puertos) |

Los wireframes de las pantallas principales están en `../Wireframes/`. El archivo `00_todas_las_pantallas.png` reúne las 12 pantallas en una sola hoja.
