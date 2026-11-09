-- campos usados para controlar la publicacion en el portal externo
ALTER TABLE Solicitudes
    ADD COLUMN IF NOT EXISTS PublicadaExterna TINYINT(1) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS FechaVencimientoPublicacion DATE NULL,
    ADD COLUMN IF NOT EXISTS IdParticipacionAprobada INT NULL;

-- proveedores que participan desde el sitio WordPress
CREATE TABLE IF NOT EXISTS Proveedores (
    IdProveedor INT NOT NULL AUTO_INCREMENT,
    CedulaJuridica VARCHAR(20) NOT NULL,
    NombreProveedor VARCHAR(150) NOT NULL,
    NombreRepresentante VARCHAR(150) NOT NULL,
    Telefono VARCHAR(30) NOT NULL,
    Correo VARCHAR(150) NOT NULL,
    PRIMARY KEY (IdProveedor),
    UNIQUE KEY uq_proveedores_cedula (CedulaJuridica)
) ENGINE=InnoDB;

-- cada proveedor puede presentar una propuesta para una solicitud
CREATE TABLE IF NOT EXISTS ParticipacionesSolicitud (
    IdParticipacion INT NOT NULL AUTO_INCREMENT,
    IdSolicitud INT NOT NULL,
    IdProveedor INT NOT NULL,
    FechaParticipacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Estado VARCHAR(20) NOT NULL DEFAULT 'Pendiente',
    PRIMARY KEY (IdParticipacion),
    UNIQUE KEY uq_participacion_proveedor_solicitud (IdSolicitud, IdProveedor),
    CONSTRAINT fk_participaciones_solicitud FOREIGN KEY (IdSolicitud)
        REFERENCES Solicitudes (IdSolicitud),
    CONSTRAINT fk_participaciones_proveedor FOREIGN KEY (IdProveedor)
        REFERENCES Proveedores (IdProveedor)
) ENGINE=InnoDB;

-- detalle independiente de la propuesta enviada por cada proveedor
CREATE TABLE IF NOT EXISTS DesglosesParticipacion (
    IdDesgloseParticipacion INT NOT NULL AUTO_INCREMENT,
    IdParticipacion INT NOT NULL,
    Mes INT NOT NULL,
    Anio INT NOT NULL,
    Horas DECIMAL(10,2) NOT NULL,
    Monto DECIMAL(14,2) NOT NULL,
    PorcentajeCobro DECIMAL(5,2) NOT NULL,
    Observaciones VARCHAR(500) NOT NULL,
    PRIMARY KEY (IdDesgloseParticipacion),
    CONSTRAINT fk_desgloses_participacion FOREIGN KEY (IdParticipacion)
        REFERENCES ParticipacionesSolicitud (IdParticipacion) ON DELETE CASCADE
) ENGINE=InnoDB;

-- una solicitud solo puede tener una propuesta aprobada
ALTER TABLE Solicitudes
    ADD CONSTRAINT fk_solicitudes_participacion_aprobada
        FOREIGN KEY (IdParticipacionAprobada)
        REFERENCES ParticipacionesSolicitud (IdParticipacion);
