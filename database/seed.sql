-- =============================================================
--  GOLDEN GYM IBAGUÉ - Datos iniciales (seed)
--  Ejecutar DESPUÉS de schema.sql
--
--  Usuarios de prueba (contraseñas guardadas con hash BCrypt):
--    Administrador : admin@goldengym.com          / Admin123*
--    Entrenador    : carlos.arias@goldengym.com   / Entrenador123*
--    Entrenador    : laura.mendez@goldengym.com   / Entrenador123*
--    Cliente       : andres.cliente@goldengym.com / Cliente123*
--    Cliente       : johana.saavedra@goldengym.com/ Cliente123*
--    Cliente       : miguel.rojas@goldengym.com   / Cliente123*
--  Las fechas se calculan con CURDATE() para que la demo siempre
--  tenga membresías vigentes y vencidas.
-- =============================================================
USE goldengym_db;

INSERT INTO Roles (IdRol, Nombre, Descripcion) VALUES
 (1, 'Administrador', 'Gestiona usuarios, planes, membresías y toda la información del gimnasio'),
 (2, 'Entrenador',    'Gestiona ejercicios, rutinas, asistencia y progreso de los clientes'),
 (3, 'Cliente',       'Consulta su membresía, rutinas, asistencias y progreso');

INSERT INTO Usuarios (IdUsuario, IdRol, Nombres, Apellidos, Correo, PasswordHash, Telefono, Documento, FechaNacimiento, Activo) VALUES
 (1, 1, 'Administrador', 'Golden Gym', 'admin@goldengym.com',
     '$2a$11$qzhAe5SqzESzAOSec3QkRe2Z8pfSWoYF/3GN4vCXW3il1ra5iMRIS', '3001112233', '1000000001', '1990-01-15', 1),
 (2, 2, 'Carlos', 'Arias', 'carlos.arias@goldengym.com',
     '$2a$11$1nqH1Q6XzpyWS3Gvm97FyeBK0D.amguYo1IiAo6a7onD/mrmystqy', '3104445566', '1000000002', '1988-04-04', 1),
 (3, 2, 'Laura', 'Méndez', 'laura.mendez@goldengym.com',
     '$2a$11$1nqH1Q6XzpyWS3Gvm97FyeBK0D.amguYo1IiAo6a7onD/mrmystqy', '3157778899', '1000000003', '1994-09-21', 1),
 (4, 3, 'Andrés', 'Cliente', 'andres.cliente@goldengym.com',
     '$2a$11$odg4yH92W3ydmCsccW38POF9trUZCxYf4VDBipH96xbKlIGYE.X4a', '3201234567', '1000000004', '2001-06-10', 1),
 (5, 3, 'Johana Jazmín', 'Saavedra Tafur', 'johana.saavedra@goldengym.com',
     '$2a$11$odg4yH92W3ydmCsccW38POF9trUZCxYf4VDBipH96xbKlIGYE.X4a', '3009876543', '1000000005', '1999-11-30', 1),
 (6, 3, 'Miguel', 'Rojas', 'miguel.rojas@goldengym.com',
     '$2a$11$odg4yH92W3ydmCsccW38POF9trUZCxYf4VDBipH96xbKlIGYE.X4a', '3112223344', '1000000006', '1997-02-18', 1);

-- Planes tomados de la página web de Golden Gym Ibagué
INSERT INTO Planes (IdPlan, Nombre, Descripcion, DuracionDias, Precio, Activo) VALUES
 (1, 'Rutina por Sesión', 'Acceso por día a todas las áreas del gimnasio con atención personalizada, sin compromiso', 1, 15000, 1),
 (2, 'Plan Semanal',      'Acceso ilimitado por 7 días, todas las áreas, clases de cardio-rumba y asesoría de entrenamiento', 7, 40000, 1),
 (3, 'Plan Quincenal',    'Acceso ilimitado por 15 días, todas las áreas, clases de cardio-rumba y asesoría de entrenamiento', 15, 55000, 1),
 (4, 'Plan Mensual',      'Acceso ilimitado por 30 días, cardio-rumba, asesoría personalizada y plan de entrenamiento', 30, 65000, 1);

-- FechaFin = FechaInicio + DuracionDias - 1
INSERT INTO Membresias (IdUsuario, IdPlan, FechaInicio, FechaFin, PrecioPagado, Estado) VALUES
 (4, 4, DATE_SUB(CURDATE(), INTERVAL 10 DAY), DATE_ADD(DATE_SUB(CURDATE(), INTERVAL 10 DAY), INTERVAL 29 DAY), 65000, 'Activa'),
 (4, 2, DATE_SUB(CURDATE(), INTERVAL 40 DAY), DATE_ADD(DATE_SUB(CURDATE(), INTERVAL 40 DAY), INTERVAL 6 DAY),  40000, 'Activa'),
 (5, 3, DATE_SUB(CURDATE(), INTERVAL 3 DAY),  DATE_ADD(DATE_SUB(CURDATE(), INTERVAL 3 DAY),  INTERVAL 14 DAY), 55000, 'Activa'),
 (6, 2, DATE_SUB(CURDATE(), INTERVAL 20 DAY), DATE_ADD(DATE_SUB(CURDATE(), INTERVAL 20 DAY), INTERVAL 6 DAY),  40000, 'Activa');

INSERT INTO Ejercicios (IdEjercicio, Nombre, GrupoMuscular, Descripcion) VALUES
 (1,  'Press de banca',          'Pecho',     'Barra o mancuernas en banco plano'),
 (2,  'Aperturas con mancuernas','Pecho',     'Banco plano, codos semiflexionados'),
 (3,  'Sentadilla libre',        'Piernas',   'Barra sobre trapecios, bajar hasta 90°'),
 (4,  'Prensa de piernas',       'Piernas',   'Máquina de prensa 45°'),
 (5,  'Peso muerto',             'Espalda',   'Barra desde el suelo con espalda neutra'),
 (6,  'Jalón al pecho',          'Espalda',   'Polea alta con agarre abierto'),
 (7,  'Press militar',           'Hombros',   'Barra o mancuernas sentado'),
 (8,  'Curl de bíceps',          'Brazos',    'Mancuernas alternas de pie'),
 (9,  'Fondos en paralelas',     'Brazos',    'Tríceps con peso corporal'),
 (10, 'Plancha abdominal',       'Abdomen',   'Isométrico, repeticiones en segundos'),
 (11, 'Bicicleta estática',      'Cardio',    'Intervalos de intensidad moderada'),
 (12, 'Burpees',                 'Funcional', 'Ejercicio funcional de cuerpo completo');

INSERT INTO Rutinas (IdRutina, Nombre, Objetivo, Nivel, IdCliente, IdEntrenador) VALUES
 (1, 'Fuerza tren superior', 'Ganar fuerza en pecho, espalda y brazos', 'Intermedio',   4, 2),
 (2, 'Piernas y abdomen',    'Tonificar tren inferior y core',          'Principiante', 4, 2),
 (3, 'Quema de grasa',       'Mejorar resistencia y bajar % de grasa',  'Principiante', 5, 3);

INSERT INTO RutinaEjercicio (IdRutina, IdEjercicio, Series, Repeticiones, DescansoSeg, Orden) VALUES
 (1, 1, 4, 10, 90, 1), (1, 6, 4, 12, 60, 2), (1, 7, 3, 10, 60, 3), (1, 8, 3, 12, 45, 4),
 (2, 3, 4, 12, 90, 1), (2, 4, 3, 15, 60, 2), (2, 10, 3, 40, 30, 3),
 (3, 11, 1, 20, 0, 1), (3, 12, 4, 15, 45, 2), (3, 10, 3, 30, 30, 3);

INSERT INTO Asistencias (IdUsuario, FechaEntrada, FechaSalida, IdRegistradoPor) VALUES
 (4, DATE_SUB(NOW(), INTERVAL 3 DAY), DATE_ADD(DATE_SUB(NOW(), INTERVAL 3 DAY), INTERVAL 90 MINUTE), 2),
 (4, DATE_SUB(NOW(), INTERVAL 2 DAY), DATE_ADD(DATE_SUB(NOW(), INTERVAL 2 DAY), INTERVAL 75 MINUTE), 4),
 (4, DATE_SUB(NOW(), INTERVAL 1 DAY), DATE_ADD(DATE_SUB(NOW(), INTERVAL 1 DAY), INTERVAL 60 MINUTE), 4),
 (5, DATE_SUB(NOW(), INTERVAL 1 DAY), DATE_ADD(DATE_SUB(NOW(), INTERVAL 1 DAY), INTERVAL 80 MINUTE), 3);

INSERT INTO Progresos (IdUsuario, IdEntrenador, Fecha, PesoKg, EstaturaCm, GrasaCorporal, Observaciones) VALUES
 (4, 2, DATE_SUB(CURDATE(), INTERVAL 30 DAY), 78.50, 175, 22.0, 'Valoración inicial'),
 (4, 2, DATE_SUB(CURDATE(), INTERVAL 15 DAY), 77.20, 175, 21.1, 'Buen avance en fuerza'),
 (4, 2, CURDATE(),                            76.40, 175, 20.3, 'Mejoró resistencia cardiovascular'),
 (5, 3, DATE_SUB(CURDATE(), INTERVAL 3 DAY),  62.00, 162, 27.5, 'Valoración inicial');
