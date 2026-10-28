-- Ejecutar solo en la base de pruebas del modulo externo
-- Agrega la informacion que permite controlar la publicacion en WordPress

ALTER TABLE Solicitudes
    ADD COLUMN IF NOT EXISTS PublicadaExterna TINYINT(1) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS FechaVencimientoPublicacion DATE NULL;
