-- =============================================================
--  GOLDEN GYM IBAGUÉ - Proyecto Final Integrador (Grupo 9 · Aplicación GYM)
--  Empresa desarrolladora: FitCore Technologies S.A.S.
--  Motor: MySQL / MariaDB (XAMPP 8.x - MariaDB 10.4+)
--  Uso: importar en phpMyAdmin o ejecutar:  mysql -u root < schema.sql
-- =============================================================

DROP DATABASE IF EXISTS goldengym_db;
CREATE DATABASE goldengym_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE goldengym_db;

-- -------------------------------------------------------------
-- 1. Roles: Administrador, Entrenador, Cliente
-- -------------------------------------------------------------
CREATE TABLE Roles (
    IdRol        INT          NOT NULL AUTO_INCREMENT,
    Nombre       VARCHAR(30)  NOT NULL,
    Descripcion  VARCHAR(150) NULL,
    CONSTRAINT PK_Roles PRIMARY KEY (IdRol),
    CONSTRAINT UQ_Roles_Nombre UNIQUE (Nombre)
) ENGINE=InnoDB;

-- -------------------------------------------------------------
-- 2. Usuarios (afiliados, entrenadores y administradores)
-- -------------------------------------------------------------
CREATE TABLE Usuarios (
    IdUsuario        INT          NOT NULL AUTO_INCREMENT,
    IdRol            INT          NOT NULL,
    Nombres          VARCHAR(80)  NOT NULL,
    Apellidos        VARCHAR(80)  NOT NULL,
    Correo           VARCHAR(120) NOT NULL,
    PasswordHash     VARCHAR(255) NOT NULL,
    Telefono         VARCHAR(20)  NULL,
    Documento        VARCHAR(20)  NULL,
    FechaNacimiento  DATE         NULL,
    Activo           TINYINT(1)   NOT NULL DEFAULT 1,
    FechaRegistro    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_Usuarios PRIMARY KEY (IdUsuario),
    CONSTRAINT UQ_Usuarios_Correo UNIQUE (Correo),
    CONSTRAINT UQ_Usuarios_Documento UNIQUE (Documento),
    CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (IdRol)
        REFERENCES Roles (IdRol) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB;

-- -------------------------------------------------------------
-- 3. Planes (catálogo de tarifas: sesión, semanal, quincenal, mensual)
-- -------------------------------------------------------------
CREATE TABLE Planes (
    IdPlan        INT           NOT NULL AUTO_INCREMENT,
    Nombre        VARCHAR(60)   NOT NULL,
    Descripcion   VARCHAR(300)  NULL,
    DuracionDias  INT           NOT NULL,
    Precio        DECIMAL(10,2) NOT NULL,
    Activo        TINYINT(1)    NOT NULL DEFAULT 1,
    CONSTRAINT PK_Planes PRIMARY KEY (IdPlan),
    CONSTRAINT UQ_Planes_Nombre UNIQUE (Nombre),
    CONSTRAINT CK_Planes_Duracion CHECK (DuracionDias > 0),
    CONSTRAINT CK_Planes_Precio CHECK (Precio >= 0)
) ENGINE=InnoDB;

-- -------------------------------------------------------------
-- 4. Membresias (plan adquirido por un cliente)
--    Estado guardado: Activa | Cancelada
--    Estado calculado en la API: Vigente | Pendiente | Vencida | Cancelada
-- -------------------------------------------------------------
CREATE TABLE Membresias (
    IdMembresia    INT           NOT NULL AUTO_INCREMENT,
    IdUsuario      INT           NOT NULL,
    IdPlan         INT           NOT NULL,
    FechaInicio    DATE          NOT NULL,
    FechaFin       DATE          NOT NULL,
    PrecioPagado   DECIMAL(10,2) NOT NULL,
    Estado         VARCHAR(15)   NOT NULL DEFAULT 'Activa',
    FechaRegistro  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_Membresias PRIMARY KEY (IdMembresia),
    CONSTRAINT FK_Membresias_Usuarios FOREIGN KEY (IdUsuario)
        REFERENCES Usuarios (IdUsuario) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT FK_Membresias_Planes FOREIGN KEY (IdPlan)
        REFERENCES Planes (IdPlan) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT CK_Membresias_Fechas CHECK (FechaFin >= FechaInicio),
    CONSTRAINT CK_Membresias_Precio CHECK (PrecioPagado >= 0),
    CONSTRAINT CK_Membresias_Estado CHECK (Estado IN ('Activa', 'Cancelada'))
) ENGINE=InnoDB;

CREATE INDEX IX_Membresias_Usuario_Fechas ON Membresias (IdUsuario, FechaInicio, FechaFin);

-- -------------------------------------------------------------
-- 5. Ejercicios (catálogo)
-- -------------------------------------------------------------
CREATE TABLE Ejercicios (
    IdEjercicio    INT          NOT NULL AUTO_INCREMENT,
    Nombre         VARCHAR(80)  NOT NULL,
    GrupoMuscular  VARCHAR(40)  NOT NULL,
    Descripcion    VARCHAR(300) NULL,
    Activo         TINYINT(1)   NOT NULL DEFAULT 1,
    CONSTRAINT PK_Ejercicios PRIMARY KEY (IdEjercicio),
    CONSTRAINT UQ_Ejercicios_Nombre UNIQUE (Nombre)
) ENGINE=InnoDB;

-- -------------------------------------------------------------
-- 6. Rutinas (asignadas por un entrenador a un cliente)
-- -------------------------------------------------------------
CREATE TABLE Rutinas (
    IdRutina       INT          NOT NULL AUTO_INCREMENT,
    Nombre         VARCHAR(80)  NOT NULL,
    Objetivo       VARCHAR(200) NULL,
    Nivel          VARCHAR(15)  NOT NULL,
    IdCliente      INT          NOT NULL,
    IdEntrenador   INT          NOT NULL,
    FechaCreacion  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Activa         TINYINT(1)   NOT NULL DEFAULT 1,
    CONSTRAINT PK_Rutinas PRIMARY KEY (IdRutina),
    CONSTRAINT FK_Rutinas_Cliente FOREIGN KEY (IdCliente)
        REFERENCES Usuarios (IdUsuario) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT FK_Rutinas_Entrenador FOREIGN KEY (IdEntrenador)
        REFERENCES Usuarios (IdUsuario) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT CK_Rutinas_Nivel CHECK (Nivel IN ('Principiante', 'Intermedio', 'Avanzado'))
) ENGINE=InnoDB;

-- -------------------------------------------------------------
-- 7. RutinaEjercicio (tabla intermedia N:M)
-- -------------------------------------------------------------
CREATE TABLE RutinaEjercicio (
    IdRutina      INT NOT NULL,
    IdEjercicio   INT NOT NULL,
    Series        INT NOT NULL,
    Repeticiones  INT NOT NULL,
    DescansoSeg   INT NOT NULL DEFAULT 60,
    Orden         INT NOT NULL DEFAULT 1,
    CONSTRAINT PK_RutinaEjercicio PRIMARY KEY (IdRutina, IdEjercicio),
    CONSTRAINT FK_RutinaEjercicio_Rutinas FOREIGN KEY (IdRutina)
        REFERENCES Rutinas (IdRutina) ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT FK_RutinaEjercicio_Ejercicios FOREIGN KEY (IdEjercicio)
        REFERENCES Ejercicios (IdEjercicio) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT CK_RutinaEjercicio_Series CHECK (Series BETWEEN 1 AND 20),
    CONSTRAINT CK_RutinaEjercicio_Reps CHECK (Repeticiones BETWEEN 1 AND 100),
    CONSTRAINT CK_RutinaEjercicio_Descanso CHECK (DescansoSeg BETWEEN 0 AND 600)
) ENGINE=InnoDB;

-- -------------------------------------------------------------
-- 8. Asistencias (registro de entrada / salida al gimnasio)
-- -------------------------------------------------------------
CREATE TABLE Asistencias (
    IdAsistencia     INT      NOT NULL AUTO_INCREMENT,
    IdUsuario        INT      NOT NULL,
    FechaEntrada     DATETIME NOT NULL,
    FechaSalida      DATETIME NULL,
    IdRegistradoPor  INT      NULL,
    CONSTRAINT PK_Asistencias PRIMARY KEY (IdAsistencia),
    CONSTRAINT FK_Asistencias_Usuario FOREIGN KEY (IdUsuario)
        REFERENCES Usuarios (IdUsuario) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT FK_Asistencias_RegistradoPor FOREIGN KEY (IdRegistradoPor)
        REFERENCES Usuarios (IdUsuario) ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT CK_Asistencias_Fechas CHECK (FechaSalida IS NULL OR FechaSalida >= FechaEntrada)
) ENGINE=InnoDB;

CREATE INDEX IX_Asistencias_Usuario_Fecha ON Asistencias (IdUsuario, FechaEntrada);

-- -------------------------------------------------------------
-- 9. Progresos (mediciones físicas del cliente)
-- -------------------------------------------------------------
CREATE TABLE Progresos (
    IdProgreso      INT          NOT NULL AUTO_INCREMENT,
    IdUsuario       INT          NOT NULL,
    IdEntrenador    INT          NULL,
    Fecha           DATE         NOT NULL,
    PesoKg          DECIMAL(5,2) NOT NULL,
    EstaturaCm      DECIMAL(5,2) NULL,
    GrasaCorporal   DECIMAL(4,1) NULL,
    Observaciones   VARCHAR(300) NULL,
    CONSTRAINT PK_Progresos PRIMARY KEY (IdProgreso),
    CONSTRAINT FK_Progresos_Usuario FOREIGN KEY (IdUsuario)
        REFERENCES Usuarios (IdUsuario) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT FK_Progresos_Entrenador FOREIGN KEY (IdEntrenador)
        REFERENCES Usuarios (IdUsuario) ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT CK_Progresos_Peso CHECK (PesoKg BETWEEN 20 AND 400),
    CONSTRAINT CK_Progresos_Estatura CHECK (EstaturaCm IS NULL OR EstaturaCm BETWEEN 50 AND 250),
    CONSTRAINT CK_Progresos_Grasa CHECK (GrasaCorporal IS NULL OR GrasaCorporal BETWEEN 2 AND 70)
) ENGINE=InnoDB;

CREATE INDEX IX_Progresos_Usuario_Fecha ON Progresos (IdUsuario, Fecha);
