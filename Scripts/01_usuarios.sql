-- tabla base para las cuentas que pueden entrar al sistema en SQL Server
CREATE TABLE dbo.Usuarios
(
    IdUsuario INT IDENTITY(1,1) NOT NULL,
    NombreCompleto VARCHAR(120) NOT NULL,
    NombreUsuario VARCHAR(50) NOT NULL,
    Correo VARCHAR(120) NOT NULL,
    TipoUsuario VARCHAR(20) NOT NULL CONSTRAINT DF_Usuarios_Tipo DEFAULT 'Usuario',
    Estado VARCHAR(20) NOT NULL CONSTRAINT DF_Usuarios_Estado DEFAULT 'Activo',
    ContrasenaCifrada VARCHAR(MAX) NOT NULL,
    Nonce VARBINARY(12) NOT NULL,
    EtiquetaAutenticacion VARBINARY(16) NOT NULL,
    IntentosFallidos INT NOT NULL CONSTRAINT DF_Usuarios_Intentos DEFAULT 0,

    CONSTRAINT PK_Usuarios PRIMARY KEY (IdUsuario),
    CONSTRAINT UQ_Usuarios_Nombre UNIQUE (NombreUsuario),
    CONSTRAINT UQ_Usuarios_Correo UNIQUE (Correo)
);
