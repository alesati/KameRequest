using Dapper;
using KameRequest.Models;

namespace KameRequest.Repository.Repositories;

public class RepresentanteRepository(IDbConnectionFactory conexiones)
{
    // obtiene los registros ordenados para mostrarlos en pantalla
    public IReadOnlyList<Representante> Listar()
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "SELECT IdRepresentante, Nombre, Email FROM Representantes ORDER BY Nombre, IdRepresentante;";
        return conexion.Query<Representante>(sql).ToList();
    }

    // obtiene solo los registros de la pagina solicitada
    public IReadOnlyList<Representante> ObtenerPagina(int pagina, int tamano)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "SELECT IdRepresentante, Nombre, Email FROM Representantes ORDER BY Nombre, IdRepresentante LIMIT @tamano OFFSET @offset;";
        return conexion.Query<Representante>(sql, new { tamano, offset = (pagina - 1) * tamano }).ToList();
    }

    // cuenta los registros para mostrar la paginacion
    public int Contar()
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Representantes;");
    }

    // busca un registro por su identificador
    public Representante? Obtener(int id)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "SELECT IdRepresentante, Nombre, Email FROM Representantes WHERE IdRepresentante = @id;";
        return conexion.QuerySingleOrDefault<Representante>(sql, new { id });
    }

    // guarda un nuevo registro en la base de datos
    public int Crear(Representante representante)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "INSERT INTO Representantes (Nombre, Email) VALUES (@Nombre, @Email);";
        conexion.Execute(sql, representante);
        return conexion.ExecuteScalar<int>("SELECT LAST_INSERT_ID();");
    }

    // guarda los cambios realizados al registro
    public void Actualizar(Representante representante)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "UPDATE Representantes SET Nombre = @Nombre, Email = @Email WHERE IdRepresentante = @IdRepresentante;";
        conexion.Execute(sql, representante);
    }

    // elimina el registro indicado si no tiene relaciones
    public int Eliminar(int id)
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.Execute("DELETE FROM Representantes WHERE IdRepresentante = @id;", new { id });
    }
}


