using Dapper;
using KameRequest.Models;

namespace KameRequest.Repository.Repositories;

public class SolicitudRepository(IDbConnectionFactory conexiones)
{
    private const string ConsultaCompleta = """
        SELECT s.IdSolicitud, s.ConsecutivoOficio, s.DocumentoRespuesta, s.DocumentoInicio,
               s.Titulo, s.Descripcion, s.IdRepresentante, s.Observaciones, s.IdEstado,
               s.FechaIngreso, s.FechaRespuesta, s.FechaInicio, e.Nombre AS Estado,
               r.Nombre AS Representante, r.Email AS CorreoRepresentante
        FROM Solicitudes s
        INNER JOIN EstadosSolicitud e ON e.IdEstado = s.IdEstado
        LEFT JOIN Representantes r ON r.IdRepresentante = s.IdRepresentante
        """;
    public Solicitud? Obtener(int id)
    {
        using var conexion = conexiones.CreateConnection();
        const string condicion = " WHERE s.IdSolicitud = @id;";
        return conexion.QuerySingleOrDefault<Solicitud>(ConsultaCompleta + condicion, new { id });
    }

    public IReadOnlyList<Solicitud> ObtenerPagina(int pagina, int tamano)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT s.IdSolicitud, s.ConsecutivoOficio, s.Titulo, s.IdEstado,
                   s.FechaIngreso, e.Nombre AS Estado, r.Nombre AS Representante, r.Email AS CorreoRepresentante
            FROM Solicitudes s
            INNER JOIN EstadosSolicitud e ON e.IdEstado = s.IdEstado
            LEFT JOIN Representantes r ON r.IdRepresentante = s.IdRepresentante
            ORDER BY s.FechaIngreso DESC, s.IdSolicitud DESC
            LIMIT @tamano OFFSET @offset;
            """;
        return conexion.Query<Solicitud>(sql, new { tamano, offset = (pagina - 1) * tamano }).ToList();
    }

    public int Contar()
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Solicitudes;");
    }

    public int ObtenerIdEstadoNueva()
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "SELECT IdEstado FROM EstadosSolicitud WHERE Nombre = 'Nueva' LIMIT 1;";
        return conexion.ExecuteScalar<int>(sql);
    }

    public int Crear(Solicitud solicitud)
    {
        using var conexion = conexiones.CreateConnection();
        var estado = ObtenerIdEstadoNueva();
        const string sql = """
            INSERT INTO Solicitudes
                (ConsecutivoOficio, DocumentoRespuesta, DocumentoInicio, Titulo, Descripcion,
                 IdRepresentante, Observaciones, IdEstado, FechaIngreso, FechaRespuesta, FechaInicio)
            VALUES
                (@ConsecutivoOficio, @DocumentoRespuesta, @DocumentoInicio, @Titulo, @Descripcion,
                 @IdRepresentante, @Observaciones, @estado, @FechaIngreso, @FechaRespuesta, @FechaInicio);
            """;
        conexion.Execute(sql, new { solicitud.ConsecutivoOficio, solicitud.DocumentoRespuesta, solicitud.DocumentoInicio, solicitud.Titulo, Descripcion = solicitud.Descripcion ?? string.Empty, solicitud.IdRepresentante, solicitud.Observaciones, estado, solicitud.FechaIngreso, solicitud.FechaRespuesta, solicitud.FechaInicio });
        return conexion.ExecuteScalar<int>("SELECT LAST_INSERT_ID();");
    }

    public void Actualizar(Solicitud solicitud)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            UPDATE Solicitudes
            SET ConsecutivoOficio = @ConsecutivoOficio,
                DocumentoRespuesta = @DocumentoRespuesta,
                DocumentoInicio = @DocumentoInicio,
                Titulo = @Titulo,
                Descripcion = @Descripcion,
                IdRepresentante = @IdRepresentante,
                Observaciones = @Observaciones,
                IdEstado = @IdEstado,
                FechaIngreso = @FechaIngreso,
                FechaRespuesta = @FechaRespuesta,
                FechaInicio = @FechaInicio
            WHERE IdSolicitud = @IdSolicitud;
            """;
        conexion.Execute(sql, new { solicitud.IdSolicitud, solicitud.ConsecutivoOficio, solicitud.DocumentoRespuesta, solicitud.DocumentoInicio, solicitud.Titulo, Descripcion = solicitud.Descripcion ?? string.Empty, solicitud.IdRepresentante, solicitud.Observaciones, solicitud.IdEstado, solicitud.FechaIngreso, solicitud.FechaRespuesta, solicitud.FechaInicio });
    }

    public int Eliminar(int id)
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.Execute("DELETE FROM Solicitudes WHERE IdSolicitud = @id;", new { id });
    }

    public IReadOnlyList<EstadoSolicitud> ListarEstados()
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "SELECT IdEstado, Nombre FROM EstadosSolicitud ORDER BY IdEstado;";
        return conexion.Query<EstadoSolicitud>(sql).ToList();
    }

    public IReadOnlyList<Solicitud> SinRespuestaNoVencidas(int idEstadoVencida)
    {
        using var conexion = conexiones.CreateConnection();
        const string condicion = " WHERE s.FechaRespuesta IS NULL AND s.IdEstado <> @idEstadoVencida;";
        return conexion.Query<Solicitud>(ConsultaCompleta + condicion, new { idEstadoVencida }).ToList();
    }

    public int? ObtenerIdEstadoPorNombre(string nombre)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "SELECT IdEstado FROM EstadosSolicitud WHERE Nombre = @nombre LIMIT 1;";
        return conexion.QuerySingleOrDefault<int?>(sql, new { nombre });
    }

    public int MarcarVencida(int idSolicitud, int idEstadoVencida)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            UPDATE Solicitudes
            SET IdEstado = @idEstadoVencida
            WHERE IdSolicitud = @idSolicitud
              AND FechaRespuesta IS NULL
              AND IdEstado <> @idEstadoVencida;
            """;
        return conexion.Execute(sql, new { idSolicitud, idEstadoVencida });
    }

    public IReadOnlyList<Solicitud> ListarParaReporte()
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "SELECT IdSolicitud, ConsecutivoOficio, Titulo FROM Solicitudes ORDER BY IdSolicitud DESC;";
        return conexion.Query<Solicitud>(sql).ToList();
    }
}


