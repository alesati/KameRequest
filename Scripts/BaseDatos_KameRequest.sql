/*
    Ejecutar este script sobre la base de datos creada desde Plesk.
    En Plesk primero se crea una base llamada KameRequest y luego se abre
    SQL Server Management Studio o el administrador SQL para ejecutar el script.
    El script es para una base nueva y no elimina informacion existente.
*/

CREATE TABLE dbo.Usuarios
(
    IdUsuario INT IDENTITY(1,1) NOT NULL,
    NombreCompleto VARCHAR(150) NOT NULL,
    NombreUsuario VARCHAR(50) NOT NULL,
    Correo VARCHAR(150) NOT NULL,
    TipoUsuario VARCHAR(20) NOT NULL CONSTRAINT DF_Usuarios_Tipo DEFAULT 'Usuario',
    Estado VARCHAR(20) NOT NULL CONSTRAINT DF_Usuarios_Estado DEFAULT 'Activo',
    ContrasenaCifrada VARCHAR(MAX) NOT NULL,
    Nonce VARBINARY(12) NOT NULL,
    EtiquetaAutenticacion VARBINARY(16) NOT NULL,
    IntentosFallidos INT NOT NULL CONSTRAINT DF_Usuarios_Intentos DEFAULT 0,

    CONSTRAINT PK_Usuarios PRIMARY KEY (IdUsuario),
    CONSTRAINT UQ_Usuarios_Nombre UNIQUE (NombreUsuario),
    CONSTRAINT UQ_Usuarios_Correo UNIQUE (Correo),
    CONSTRAINT CK_Usuarios_Estado CHECK (Estado IN ('Activo', 'Inactivo', 'Bloqueado')),
    CONSTRAINT CK_Usuarios_Tipo CHECK (TipoUsuario IN ('Administrador', 'Usuario')),
    CONSTRAINT CK_Usuarios_Intentos CHECK (IntentosFallidos >= 0)
);
GO

CREATE TABLE dbo.Representantes
(
    IdRepresentante INT IDENTITY(1,1) NOT NULL,
    Nombre VARCHAR(150) NOT NULL,
    Email VARCHAR(150) NOT NULL,

    CONSTRAINT PK_Representantes PRIMARY KEY (IdRepresentante)
);
GO

CREATE TABLE dbo.EstadosSolicitud
(
    IdEstado INT IDENTITY(1,1) NOT NULL,
    Nombre VARCHAR(50) NOT NULL,

    CONSTRAINT PK_EstadosSolicitud PRIMARY KEY (IdEstado),
    CONSTRAINT UQ_EstadosSolicitud_Nombre UNIQUE (Nombre)
);
GO

INSERT INTO dbo.EstadosSolicitud (Nombre)
VALUES
    ('Nueva'),
    ('Respondida'),
    ('Iniciada'),
    ('Finalizada'),
    ('Vencida');
GO

CREATE TABLE dbo.Solicitudes
(
    IdSolicitud INT IDENTITY(1,1) NOT NULL,
    ConsecutivoOficio VARCHAR(50) NOT NULL,
    DocumentoRespuesta VARCHAR(255) NULL,
    DocumentoInicio VARCHAR(255) NULL,
    Titulo VARCHAR(200) NOT NULL,
    Descripcion VARCHAR(MAX) NOT NULL,
    IdRepresentante INT NULL,
    Observaciones VARCHAR(MAX) NULL,
    IdEstado INT NOT NULL CONSTRAINT DF_Solicitudes_Estado DEFAULT 1,
    FechaIngreso DATE NOT NULL,
    FechaRespuesta DATE NULL,
    FechaInicio DATE NULL,

    CONSTRAINT PK_Solicitudes PRIMARY KEY (IdSolicitud),
    CONSTRAINT UQ_Solicitudes_Consecutivo UNIQUE (ConsecutivoOficio),
    CONSTRAINT FK_Solicitudes_Representante
        FOREIGN KEY (IdRepresentante) REFERENCES dbo.Representantes (IdRepresentante),
    CONSTRAINT FK_Solicitudes_Estado
        FOREIGN KEY (IdEstado) REFERENCES dbo.EstadosSolicitud (IdEstado)
);
GO

CREATE TABLE dbo.DesglosesSolicitud
(
    IdDesglose INT IDENTITY(1,1) NOT NULL,
    IdSolicitud INT NOT NULL,
    Mes TINYINT NOT NULL,
    Anio SMALLINT NOT NULL,
    Horas DECIMAL(8,2) NOT NULL CONSTRAINT DF_Desgloses_Horas DEFAULT 0,
    Monto DECIMAL(12,2) NOT NULL CONSTRAINT DF_Desgloses_Monto DEFAULT 0,
    PorcentajeCobro DECIMAL(5,2) NOT NULL CONSTRAINT DF_Desgloses_Porcentaje DEFAULT 0,
    Observaciones VARCHAR(MAX) NULL,

    CONSTRAINT PK_DesglosesSolicitud PRIMARY KEY (IdDesglose),
    CONSTRAINT FK_DesglosesSolicitud_Solicitud
        FOREIGN KEY (IdSolicitud) REFERENCES dbo.Solicitudes (IdSolicitud),
    CONSTRAINT CK_DesglosesSolicitud_Mes CHECK (Mes BETWEEN 1 AND 12),
    CONSTRAINT CK_DesglosesSolicitud_Anio CHECK (Anio BETWEEN 1000 AND 9999),
    CONSTRAINT CK_DesglosesSolicitud_Horas CHECK (Horas >= 0),
    CONSTRAINT CK_DesglosesSolicitud_Monto CHECK (Monto >= 0),
    CONSTRAINT CK_DesglosesSolicitud_Porcentaje CHECK (PorcentajeCobro BETWEEN 0 AND 100)
);
GO

CREATE TABLE dbo.Tareas
(
    IdTarea INT IDENTITY(1,1) NOT NULL,
    IdSolicitud INT NOT NULL,
    IdDesglose INT NULL,
    IdUsuario INT NOT NULL,
    Descripcion VARCHAR(MAX) NOT NULL,
    Fecha DATE NOT NULL,
    HorasInvertidas DECIMAL(8,2) NOT NULL,

    CONSTRAINT PK_Tareas PRIMARY KEY (IdTarea),
    CONSTRAINT FK_Tareas_Solicitud
        FOREIGN KEY (IdSolicitud) REFERENCES dbo.Solicitudes (IdSolicitud),
    CONSTRAINT FK_Tareas_Desglose
        FOREIGN KEY (IdDesglose) REFERENCES dbo.DesglosesSolicitud (IdDesglose),
    CONSTRAINT FK_Tareas_Usuario
        FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuarios (IdUsuario),
    CONSTRAINT CK_Tareas_Horas CHECK (HorasInvertidas >= 0)
);
GO

CREATE TABLE dbo.Bitacoras
(
    IdBitacora INT IDENTITY(1,1) NOT NULL,
    Fecha DATETIME2 NOT NULL CONSTRAINT DF_Bitacoras_Fecha DEFAULT GETDATE(),
    IdUsuario INT NULL,
    Descripcion VARCHAR(MAX) NOT NULL,

    CONSTRAINT PK_Bitacoras PRIMARY KEY (IdBitacora),
    CONSTRAINT FK_Bitacoras_Usuario
        FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuarios (IdUsuario)
);
GO

CREATE INDEX IX_Bitacoras_Fecha ON dbo.Bitacoras (Fecha DESC);
GO



