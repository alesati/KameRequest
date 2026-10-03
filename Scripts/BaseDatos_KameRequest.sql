CREATE DATABASE IF NOT EXISTS kamerequest
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE kamerequest;

CREATE TABLE IF NOT EXISTS Usuarios (
    IdUsuario INT NOT NULL AUTO_INCREMENT,
    NombreCompleto VARCHAR(150) NOT NULL,
    NombreUsuario VARCHAR(50) NOT NULL,
    Correo VARCHAR(150) NOT NULL,
    TipoUsuario VARCHAR(20) NOT NULL DEFAULT 'Usuario',
    Estado VARCHAR(20) NOT NULL DEFAULT 'Activo',
    ContrasenaCifrada TEXT NOT NULL,
    Nonce VARBINARY(12) NOT NULL,
    EtiquetaAutenticacion VARBINARY(16) NOT NULL,
    IntentosFallidos INT NOT NULL DEFAULT 0,
    PRIMARY KEY (IdUsuario),
    CONSTRAINT uq_usuarios_nombre UNIQUE (NombreUsuario),
    CONSTRAINT uq_usuarios_correo UNIQUE (Correo),
    CONSTRAINT ck_usuarios_estado CHECK (Estado IN ('Activo', 'Inactivo', 'Bloqueado')),
    CONSTRAINT ck_usuarios_tipo CHECK (TipoUsuario IN ('Administrador', 'Usuario')),
    CONSTRAINT ck_usuarios_intentos CHECK (IntentosFallidos >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Representantes (
    IdRepresentante INT NOT NULL AUTO_INCREMENT,
    Nombre VARCHAR(150) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    PRIMARY KEY (IdRepresentante)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS EstadosSolicitud (
    IdEstado INT NOT NULL AUTO_INCREMENT,
    Nombre VARCHAR(50) NOT NULL,
    PRIMARY KEY (IdEstado),
    CONSTRAINT uq_estados_nombre UNIQUE (Nombre)
) ENGINE=InnoDB;

INSERT IGNORE INTO EstadosSolicitud (IdEstado, Nombre) VALUES
    (1, 'Nueva'),
    (2, 'Respondida'),
    (3, 'Iniciada'),
    (4, 'Finalizada'),
    (5, 'Vencida');

CREATE TABLE IF NOT EXISTS Solicitudes (
    IdSolicitud INT NOT NULL AUTO_INCREMENT,
    ConsecutivoOficio VARCHAR(50) NOT NULL,
    DocumentoRespuesta VARCHAR(255) NULL,
    DocumentoInicio VARCHAR(255) NULL,
    Titulo VARCHAR(200) NOT NULL,
    MontoTotal DECIMAL(12,2) NOT NULL,
    Descripcion TEXT NOT NULL,
    IdRepresentante INT NULL,
    Observaciones TEXT NULL,
    IdEstado INT NOT NULL DEFAULT 1,
    FechaIngreso DATE NOT NULL,
    FechaRespuesta DATE NULL,
    FechaInicio DATE NULL,
    PRIMARY KEY (IdSolicitud),
    CONSTRAINT uq_solicitudes_consecutivo UNIQUE (ConsecutivoOficio),
    CONSTRAINT fk_solicitudes_representante
        FOREIGN KEY (IdRepresentante) REFERENCES Representantes (IdRepresentante)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_solicitudes_estado
        FOREIGN KEY (IdEstado) REFERENCES EstadosSolicitud (IdEstado)
        ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS DesglosesSolicitud (
    IdDesglose INT NOT NULL AUTO_INCREMENT,
    IdSolicitud INT NOT NULL,
    Mes TINYINT NOT NULL,
    Anio SMALLINT NOT NULL,
    Horas DECIMAL(8,2) NOT NULL DEFAULT 0,
    Monto DECIMAL(12,2) NOT NULL DEFAULT 0,
    PorcentajeCobro DECIMAL(5,2) NOT NULL DEFAULT 0,
    Observaciones TEXT NULL,
    PRIMARY KEY (IdDesglose),
    CONSTRAINT fk_desgloses_solicitud
        FOREIGN KEY (IdSolicitud) REFERENCES Solicitudes (IdSolicitud)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT ck_desgloses_mes CHECK (Mes BETWEEN 1 AND 12),
    CONSTRAINT ck_desgloses_anio CHECK (Anio BETWEEN 1000 AND 9999),
    CONSTRAINT ck_desgloses_horas CHECK (Horas >= 0),
    CONSTRAINT ck_desgloses_monto CHECK (Monto >= 0),
    CONSTRAINT ck_desgloses_porcentaje CHECK (PorcentajeCobro BETWEEN 0 AND 100)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Tareas (
    IdTarea INT NOT NULL AUTO_INCREMENT,
    IdSolicitud INT NOT NULL,
    IdDesglose INT NULL,
    IdUsuario INT NOT NULL,
    Descripcion TEXT NOT NULL,
    Fecha DATE NOT NULL,
    HorasInvertidas DECIMAL(8,2) NOT NULL,
    PRIMARY KEY (IdTarea),
    CONSTRAINT fk_tareas_solicitud
        FOREIGN KEY (IdSolicitud) REFERENCES Solicitudes (IdSolicitud)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_tareas_desglose
        FOREIGN KEY (IdDesglose) REFERENCES DesglosesSolicitud (IdDesglose)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_tareas_usuario
        FOREIGN KEY (IdUsuario) REFERENCES Usuarios (IdUsuario)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT ck_tareas_horas CHECK (HorasInvertidas >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Bitacoras (
    IdBitacora INT NOT NULL AUTO_INCREMENT,
    Fecha DATETIME NOT NULL,
    IdUsuario INT NULL,
    Descripcion TEXT NOT NULL,
    PRIMARY KEY (IdBitacora),
    INDEX ix_bitacoras_fecha (Fecha),
    CONSTRAINT fk_bitacoras_usuario
        FOREIGN KEY (IdUsuario) REFERENCES Usuarios (IdUsuario)
        ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB;
