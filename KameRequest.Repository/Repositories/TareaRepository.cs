using Dapper;
using KameRequest.Models;

namespace KameRequest.Repository.Repositories;

public class TareaRepository(IDbConnectionFactory conexiones)
{
    // obtiene los registros relacionados con una solicitud y su pagina
    public IReadOnlyList<TareaSolicitud> ObtenerPaginaPorSolicitud(int idSolicitud, int pagina, int tamano)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT t.IdTarea, t.IdSolicitud, t.IdUsuario, t.IdDesglose, t.Descripcion,
                   t.Fecha, t.HorasInvertidas, u.NombreCompleto AS Usuario
            FROM Tareas t
            INNER JOIN Usuarios u ON u.IdUsuario = t.IdUsuario
            WHERE t.IdSolicitud = @idSolicitud
            ORDER BY t.Fecha DESC, t.IdTarea DESC
            LIMIT @tamano OFFSET @offset;
            """;
        return conexion.Query<TareaSolicitud>(sql, new { idSolicitud, tamano, offset = (pagina - 1) * tamano }).ToList();
    }

    // cuenta los registros que pertenecen a la solicitud
    public int ContarPorSolicitud(int idSolicitud)
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Tareas WHERE IdSolicitud = @idSolicitud;", new { idSolicitud });
    }

    // busca un registro por su identificador
    public TareaSolicitud? Obtener(int id)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT t.IdTarea, t.IdSolicitud, t.IdUsuario, t.IdDesglose, t.Descripcion,
                   t.Fecha, t.HorasInvertidas, u.NombreCompleto AS Usuario
            FROM Tareas t
            INNER JOIN Usuarios u ON u.IdUsuario = t.IdUsuario
            WHERE t.IdTarea = @id;
            """;
        return conexion.QuerySingleOrDefault<TareaSolicitud>(sql, new { id });
    }

    // guarda un nuevo registro en la base de datos
    public int Crear(TareaSolicitud tarea)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            INSERT INTO Tareas (IdSolicitud, IdUsuario, Descripcion, Fecha, HorasInvertidas)
            VALUES (@IdSolicitud, @IdUsuario, @Descripcion, @Fecha, @HorasInvertidas);
            """;
        conexion.Execute(sql, tarea);
        return conexion.ExecuteScalar<int>("SELECT LAST_INSERT_ID();");
    }

    // guarda los cambios realizados al registro
    public void Actualizar(TareaSolicitud tarea)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "UPDATE Tareas SET Descripcion = @Descripcion, Fecha = @Fecha, HorasInvertidas = @HorasInvertidas WHERE IdTarea = @IdTarea;";
        conexion.Execute(sql, tarea);
    }

    // elimina el registro indicado si no tiene relaciones
    public int Eliminar(int id)
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.Execute("DELETE FROM Tareas WHERE IdTarea = @id;", new { id });
    }
}
