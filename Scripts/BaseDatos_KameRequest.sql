-- Base de datos inicial de KameRequest
-- Este script crea la base y la tabla necesaria para usuarios e inicio de sesion

CREATE DATABASE IF NOT EXISTS kamerequest
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE kamerequest;

CREATE TABLE IF NOT EXISTS Usuarios (
    IdUsuario INT NOT NULL AUTO_INCREMENT,
    NombreCompleto VARCHAR(120) NOT NULL,
    NombreUsuario VARCHAR(50) NOT NULL,
    Correo VARCHAR(120) NOT NULL,
    TipoUsuario VARCHAR(20) NOT NULL DEFAULT 'Usuario',
    Estado VARCHAR(20) NOT NULL DEFAULT 'Activo',
    ContrasenaCifrada TEXT NOT NULL,
    Nonce VARCHAR(100) NOT NULL,
    EtiquetaAutenticacion VARCHAR(100) NOT NULL,
    IntentosFallidos INT NOT NULL DEFAULT 0,
    PRIMARY KEY (IdUsuario),
    UNIQUE KEY uq_usuarios_nombre (NombreUsuario),
    UNIQUE KEY uq_usuarios_correo (Correo)
);

-- El primer administrador se crea automaticamente al iniciar el proyecto
-- Usuario: admin
-- Contraseña: Admin123*
