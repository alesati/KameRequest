using System.Text.Json;
using Dapper;
using KameRequest.WebService.AccesoDatos.Contratos;
using KameRequest.WebService.AccesoDatos.Infraestructura;
using KameRequest.WebService.Modelos;

namespace KameRequest.WebService.AccesoDatos.Repositorios;

// consultas y registros que utiliza el portal externo
public class SolicitudesExternasRepositorio(ConexionMariaDb conexiones) : ISolicitudesExternasRepositorio
{
    // devuelve las solicitudes que wordpress puede consultar mientras esten vigentes
    public IEnumerable<SolicitudPublicadaDto> ListarPublicadas()
    {
        using var conexion = conexiones.CrearConexion();
        const string sql = """
            SELECT s.IdSolicitud, s.ConsecutivoOficio, s.Titulo,
                   r.Nombre AS Representante, s.FechaVencimientoPublicacion
            FROM Solicitudes s
            LEFT JOIN Representantes r ON r.IdRepresentante = s.IdRepresentante
            WHERE s.PublicadaExterna = 1
              AND s.FechaVencimientoPublicacion >= CURDATE()
            ORDER BY s.FechaVencimientoPublicacion, s.ConsecutivoOficio;
            """;
        var solicitudes = conexion.Query<SolicitudPublicadaDto>(sql).ToList();
        RegistrarBitacora(conexion, null, "El portal externo consultó las solicitudes publicadas.");
        return solicitudes;
    }

    // consulta una sola solicitud sin mostrar registros no publicados
    public SolicitudDetalleExternoDto? ObtenerDetalle(int idSolicitud)
    {
        using var conexion = conexiones.CrearConexion();
        const string sql = """
            SELECT s.IdSolicitud, s.ConsecutivoOficio, s.DocumentoRespuesta,
                   s.DocumentoInicio, s.Titulo, s.Descripcion,
                   r.Nombre AS Representante, s.Observaciones,
                   e.Nombre AS Estado, s.FechaIngreso, s.FechaRespuesta,
                   s.FechaInicio, s.FechaVencimientoPublicacion
            FROM Solicitudes s
            LEFT JOIN Representantes r ON r.IdRepresentante = s.IdRepresentante
            INNER JOIN EstadosSolicitud e ON e.IdEstado = s.IdEstado
            WHERE s.IdSolicitud = @idSolicitud
              AND s.PublicadaExterna = 1
              AND s.FechaVencimientoPublicacion >= CURDATE();
            """;
        var solicitud = conexion.QuerySingleOrDefault<SolicitudDetalleExternoDto>(sql, new { idSolicitud });
        RegistrarBitacora(conexion, null, $"El portal externo consultó el detalle de la solicitud {idSolicitud}.");
        return solicitud;
    }

    // reutiliza la consulta de detalle para confirmar que se puede participar
    public bool ExisteSolicitudPublicada(int idSolicitud) => ObtenerDetalle(idSolicitud) is not null;

    // busca por cedula sin exponer los demas proveedores
    public bool ExisteProveedor(string cedulaJuridica)
    {
        using var conexion = conexiones.CrearConexion();
        const string sql = "SELECT COUNT(*) FROM Proveedores WHERE CedulaJuridica = @cedulaJuridica;";
        return conexion.ExecuteScalar<int>(sql, new { cedulaJuridica }) > 0;
    }

    // busca los datos basicos del proveedor para reutilizarlo en una nueva participacion
    public ProveedorDto? ObtenerProveedor(string cedulaJuridica)
    {
        using var conexion = conexiones.CrearConexion();
        const string sql = """
            SELECT CedulaJuridica, NombreProveedor, NombreRepresentante, Telefono, Correo
            FROM Proveedores
            WHERE CedulaJuridica = @cedulaJuridica;
            """;
        return conexion.QuerySingleOrDefault<ProveedorDto>(sql, new { cedulaJuridica });
    }

    // consulta el historial de propuestas realizadas por un proveedor
    public IEnumerable<ParticipacionProveedorDto> ListarParticipacionesProveedor(string cedulaJuridica)
    {
        using var conexion = conexiones.CrearConexion();
        const string sql = """
            SELECT p.IdParticipacion, s.ConsecutivoOficio, s.Titulo,
                   p.FechaParticipacion, p.Estado
            FROM ParticipacionesSolicitud p
            INNER JOIN Proveedores pr ON pr.IdProveedor = p.IdProveedor
            INNER JOIN Solicitudes s ON s.IdSolicitud = p.IdSolicitud
            WHERE pr.CedulaJuridica = @cedulaJuridica
            ORDER BY p.FechaParticipacion DESC;
            """;
        var participaciones = conexion.Query<ParticipacionProveedorDto>(sql, new { cedulaJuridica }).ToList();
        RegistrarBitacora(conexion, null, $"El portal externo consultó participaciones del proveedor {cedulaJuridica}.");
        return participaciones;
    }

    // inserta proveedor, participacion y desgloses como una sola operacion
    public int RegistrarParticipacion(int idSolicitud, ParticipacionSolicitudDto participacion)
    {
        using var conexion = conexiones.CrearConexion();
        conexion.Open();
        using var transaccion = conexion.BeginTransaction();
        try
        {
            var idProveedor = conexion.QuerySingleOrDefault<int?>(
                "SELECT IdProveedor FROM Proveedores WHERE CedulaJuridica = @cedula;",
                new { cedula = participacion.Proveedor.CedulaJuridica }, transaccion);

            // si la cedula no existe se crea el proveedor con los datos del formulario
            if (!idProveedor.HasValue)
            {
                const string sqlProveedor = """
                    INSERT INTO Proveedores
                        (CedulaJuridica, NombreProveedor, NombreRepresentante, Telefono, Correo)
                    VALUES
                        (@CedulaJuridica, @NombreProveedor, @NombreRepresentante, @Telefono, @Correo);
                    """;
                conexion.Execute(sqlProveedor, participacion.Proveedor, transaccion);
                idProveedor = conexion.ExecuteScalar<int>("SELECT LAST_INSERT_ID();", transaction: transaccion);
            }

            const string sqlParticipacion = """
                INSERT INTO ParticipacionesSolicitud (IdSolicitud, IdProveedor)
                VALUES (@idSolicitud, @idProveedor);
                """;
            conexion.Execute(sqlParticipacion, new { idSolicitud, idProveedor }, transaccion);
            var idParticipacion = conexion.ExecuteScalar<int>("SELECT LAST_INSERT_ID();", transaction: transaccion);

            const string sqlDesglose = """
                INSERT INTO DesglosesParticipacion
                    (IdParticipacion, Mes, Anio, Horas, Monto, PorcentajeCobro, Observaciones)
                VALUES
                    (@idParticipacion, @Mes, @Anio, @Horas, @Monto, @PorcentajeCobro, @Observaciones);
                """;
            // guarda cada periodo asociado a la participacion recien creada
            foreach (var desglose in participacion.Desgloses)
            {
                conexion.Execute(sqlDesglose, new
                {
                    idParticipacion,
                    desglose.Mes,
                    desglose.Anio,
                    desglose.Horas,
                    desglose.Monto,
                    desglose.PorcentajeCobro,
                    desglose.Observaciones
                }, transaccion);
            }

            var nuevo = JsonSerializer.Serialize(new
            {
                IdParticipacion = idParticipacion,
                IdSolicitud = idSolicitud,
                participacion.Proveedor,
                participacion.Desgloses
            });
            RegistrarBitacora(conexion, transaccion, $"El portal externo creó una participación. Nuevo: {nuevo}");

            transaccion.Commit();
            return idParticipacion;
        }
        catch (MySqlConnector.MySqlException error) when (error.Number == 1062)
        {
            transaccion.Rollback();
            throw new InvalidOperationException("PARTICIPACION_DUPLICADA", error);
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    // se usa desde el control de errores para registrar problemas tecnicos
    public void RegistrarErrorTecnico(string descripcion)
    {
        using var conexion = conexiones.CrearConexion();
        RegistrarBitacora(conexion, null, "Error técnico del portal externo: " + descripcion);
    }

    // deja una evidencia de lo que se hizo desde el portal sin un usuario autenticado
    private static void RegistrarBitacora(System.Data.IDbConnection conexion, System.Data.IDbTransaction? transaccion, string descripcion)
    {
        const string sql = "INSERT INTO Bitacoras (Fecha, IdUsuario, Descripcion) VALUES (NOW(), NULL, @descripcion);";
        conexion.Execute(sql, new { descripcion }, transaccion);
    }
}
