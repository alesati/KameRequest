using Dapper;
using KameRequest.Models;

namespace KameRequest.Repository.Repositories;

public class TareaRepository(IDbConnectionFactory conexiones)
{
    public IReadOnlyList<TareaSolicitud> ListarPorSolicitud(int idSolicitud)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT t.IdTarea, t.IdSolicitud, t.IdUsuario, t.IdDesglose, t.Descripcion,
                   t.Fecha, t.HorasInvertidas, u.NombreCompleto AS Usuario
            FROM Tareas t
            INNER JOIN Usuarios u ON u.IdUsuario = t.IdUsuario
            WHERE t.IdSolicitud = @idSolicitud
            ORDER BY t.Fecha DESC, t.IdTarea DESC;
            """;
        return conexion.Query<TareaSolicitud>(sql, new { idSolicitud }).ToList();
    }

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

    public int Crear(TareaSolicitud tarea)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "INSERT INTO Tareas (IdSolicitud, IdUsuario, Descripcion, Fecha, HorasInvertidas) VALUES (@IdSolicitud, @IdUsuario, @Descripcion, @Fecha, @HorasInvertidas);";
        conexion.Execute(sql, tarea);
        return conexion.ExecuteScalar<int>("SELECT CAST(SCOPE_IDENTITY() AS int);");
    }

    public void Actualizar(TareaSolicitud tarea)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "UPDATE Tareas SET Descripcion = @Descripcion, Fecha = @Fecha, HorasInvertidas = @HorasInvertidas WHERE IdTarea = @IdTarea;";
        conexion.Execute(sql, tarea);
    }

    public int Eliminar(int id)
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.Execute("DELETE FROM Tareas WHERE IdTarea = @id;", new { id });
    }
}

