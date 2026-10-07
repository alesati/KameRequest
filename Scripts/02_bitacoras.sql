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

CREATE INDEX IX_Bitacoras_Fecha ON dbo.Bitacoras (Fecha DESC);

