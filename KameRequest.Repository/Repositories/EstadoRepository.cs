using Dapper;
using KameRequest.Models;

namespace KameRequest.Repository.Repositories;

public class EstadoRepository(IDbConnectionFactory conexiones)
{
    // obtiene solo los registros de la pagina solicitada
    public IReadOnlyList<Estado> ObtenerPagina(int pagina, int tamano)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "SELECT IdEstado AS idEstado, Nombre FROM EstadosSolicitud ORDER BY IdEstado LIMIT @tamano OFFSET @offset;";
        return conexion.Query<Estado>(sql, new { tamano, offset = (pagina - 1) * tamano }).ToList();
    }

    // cuenta los registros para mostrar la paginacion
    public int Contar()
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM EstadosSolicitud;");
    }

    // busca un registro por su identificador
    public Estado? Obtener(int idEstado)
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.QuerySingleOrDefault<Estado>("SELECT IdEstado AS idEstado, Nombre FROM EstadosSolicitud WHERE IdEstado = @idEstado;", new { idEstado });
    }

    // guarda un nuevo registro en la base de datos
    public int Crear(Estado estado)
    {
        using var conexion = conexiones.CreateConnection();
        conexion.Execute("INSERT INTO EstadosSolicitud (Nombre) VALUES (@Nombre);", estado);
        return conexion.ExecuteScalar<int>("SELECT LAST_INSERT_ID();");
    }

    // guarda los cambios realizados al registro
    public void Actualizar(Estado estado)
    {
        using var conexion = conexiones.CreateConnection();
        conexion.Execute("UPDATE EstadosSolicitud SET Nombre = @Nombre WHERE IdEstado = @idEstado;", estado);
    }

    // elimina el registro indicado si no tiene relaciones
    public int Eliminar(int idEstado)
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.Execute("DELETE FROM EstadosSolicitud WHERE IdEstado = @idEstado;", new { idEstado });
    }
}

