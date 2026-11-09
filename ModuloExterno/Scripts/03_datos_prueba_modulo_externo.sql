-- Datos unicamente para tiusr23pl_kamerequest_ext_pruebas
-- Se pueden ejecutar mas de una vez sin duplicar los registros

INSERT INTO EstadosSolicitud (Nombre)
SELECT 'Nueva'
WHERE NOT EXISTS (SELECT 1 FROM EstadosSolicitud WHERE Nombre = 'Nueva');

INSERT INTO EstadosSolicitud (Nombre)
SELECT 'Iniciada'
WHERE NOT EXISTS (SELECT 1 FROM EstadosSolicitud WHERE Nombre = 'Iniciada');

INSERT INTO Representantes (Nombre, Email)
SELECT 'Municipalidad de San Jose', 'proveeduria@msj.go.cr'
WHERE NOT EXISTS (SELECT 1 FROM Representantes WHERE Nombre = 'Municipalidad de San Jose');

INSERT INTO Representantes (Nombre, Email)
SELECT 'Condominio Vista del Valle', 'administracion@vistadelvalle.cr'
WHERE NOT EXISTS (SELECT 1 FROM Representantes WHERE Nombre = 'Condominio Vista del Valle');

-- Dos solicitudes visibles en WordPress y una que sirve para probar que no se publica
INSERT INTO Solicitudes
    (ConsecutivoOficio, DocumentoRespuesta, DocumentoInicio, Titulo, Descripcion,
     IdRepresentante, Observaciones, IdEstado, FechaIngreso, FechaRespuesta, FechaInicio,
     PublicadaExterna, FechaVencimientoPublicacion)
SELECT 'OF-EXT-2026-001', NULL, 'oficio-limpieza.pdf',
       'Servicio de limpieza para oficinas',
       'Contratacion de servicio de limpieza para las oficinas administrativas.',
       r.IdRepresentante, 'Presentar propuesta con detalle mensual.', e.IdEstado,
       '2026-10-20', NULL, NULL, 1, '2026-12-31'
FROM Representantes r
CROSS JOIN EstadosSolicitud e
WHERE r.Nombre = 'Municipalidad de San Jose'
  AND e.Nombre = 'Nueva'
  AND NOT EXISTS (SELECT 1 FROM Solicitudes WHERE ConsecutivoOficio = 'OF-EXT-2026-001');

INSERT INTO Solicitudes
    (ConsecutivoOficio, DocumentoRespuesta, DocumentoInicio, Titulo, Descripcion,
     IdRepresentante, Observaciones, IdEstado, FechaIngreso, FechaRespuesta, FechaInicio,
     PublicadaExterna, FechaVencimientoPublicacion)
SELECT 'OF-EXT-2026-002', NULL, 'oficio-jardineria.pdf',
       'Mantenimiento de zonas verdes',
       'Servicio mensual de jardineria y mantenimiento de zonas comunes.',
       r.IdRepresentante, 'Se requiere experiencia comprobable.', e.IdEstado,
       '2026-10-22', NULL, NULL, 1, '2026-11-30'
FROM Representantes r
CROSS JOIN EstadosSolicitud e
WHERE r.Nombre = 'Condominio Vista del Valle'
  AND e.Nombre = 'Nueva'
  AND NOT EXISTS (SELECT 1 FROM Solicitudes WHERE ConsecutivoOficio = 'OF-EXT-2026-002');

INSERT INTO Solicitudes
    (ConsecutivoOficio, DocumentoRespuesta, DocumentoInicio, Titulo, Descripcion,
     IdRepresentante, Observaciones, IdEstado, FechaIngreso, FechaRespuesta, FechaInicio,
     PublicadaExterna, FechaVencimientoPublicacion)
SELECT 'OF-EXT-2026-003', NULL, NULL,
       'Compra de suministros de oficina',
       'Esta solicitud no debe aparecer en el portal externo.',
       r.IdRepresentante, NULL, e.IdEstado,
       '2026-10-23', NULL, NULL, 0, NULL
FROM Representantes r
CROSS JOIN EstadosSolicitud e
WHERE r.Nombre = 'Municipalidad de San Jose'
  AND e.Nombre = 'Nueva'
  AND NOT EXISTS (SELECT 1 FROM Solicitudes WHERE ConsecutivoOficio = 'OF-EXT-2026-003');

-- Proveedor y participacion ya existentes para probar EXT6
INSERT INTO Proveedores (CedulaJuridica, NombreProveedor, NombreRepresentante, Telefono, Correo)
SELECT '3-101-456789', 'Servicios Integrales del Valle S.A.', 'Laura Rodriguez', '2222-4567', 'contacto@integralesvalle.cr'
WHERE NOT EXISTS (SELECT 1 FROM Proveedores WHERE CedulaJuridica = '3-101-456789');

INSERT INTO ParticipacionesSolicitud (IdSolicitud, IdProveedor, Estado)
SELECT s.IdSolicitud, p.IdProveedor, 'Pendiente'
FROM Solicitudes s
CROSS JOIN Proveedores p
WHERE s.ConsecutivoOficio = 'OF-EXT-2026-001'
  AND p.CedulaJuridica = '3-101-456789'
  AND NOT EXISTS (
      SELECT 1 FROM ParticipacionesSolicitud x
      WHERE x.IdSolicitud = s.IdSolicitud AND x.IdProveedor = p.IdProveedor
  );

INSERT INTO DesglosesParticipacion
    (IdParticipacion, Mes, Anio, Horas, Monto, PorcentajeCobro, Observaciones)
SELECT ps.IdParticipacion, 11, 2026, 32.00, 450000.00, 50.00, 'Servicio de noviembre'
FROM ParticipacionesSolicitud ps
INNER JOIN Solicitudes s ON s.IdSolicitud = ps.IdSolicitud
INNER JOIN Proveedores p ON p.IdProveedor = ps.IdProveedor
WHERE s.ConsecutivoOficio = 'OF-EXT-2026-001'
  AND p.CedulaJuridica = '3-101-456789'
  AND NOT EXISTS (
      SELECT 1 FROM DesglosesParticipacion d
      WHERE d.IdParticipacion = ps.IdParticipacion AND d.Mes = 11 AND d.Anio = 2026
  );

SELECT IdSolicitud, ConsecutivoOficio, Titulo, PublicadaExterna, FechaVencimientoPublicacion
FROM Solicitudes
WHERE ConsecutivoOficio LIKE 'OF-EXT-2026-%'
ORDER BY ConsecutivoOficio;
