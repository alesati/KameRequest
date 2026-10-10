-- tabla base para las cuentas que pueden entrar al sistema
CREATE TABLE IF NOT EXISTS Usuarios (
    IdUsuario INT NOT NULL AUTO_INCREMENT,
    NombreCompleto VARCHAR(120) NOT NULL,
    NombreUsuario VARCHAR(50) NOT NULL,
    Correo VARCHAR(120) NOT NULL,
    TipoUsuario VARCHAR(20) NOT NULL DEFAULT 'Usuario',
    Estado VARCHAR(20) NOT NULL DEFAULT 'Activo',
    ContrasenaCifrada TEXT NOT NULL,
    Nonce VARBINARY(12) NOT NULL,
    EtiquetaAutenticacion VARBINARY(16) NOT NULL,
    IntentosFallidos INT NOT NULL DEFAULT 0,
    PRIMARY KEY (IdUsuario),
    UNIQUE KEY uq_usuarios_nombre (NombreUsuario),
    UNIQUE KEY uq_usuarios_correo (Correo)
);
