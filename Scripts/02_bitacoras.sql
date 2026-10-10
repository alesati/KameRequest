USE kamerequest;

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

