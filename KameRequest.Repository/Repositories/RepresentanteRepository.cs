using Dapper;
using KameRequest.Models;

namespace KameRequest.Repository.Repositories;

public class RepresentanteRepository(IDbConnectionFactory conexiones)
{
    public IReadOnlyList<Representante> Listar()
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "SELECT IdRepresentante, Nombre, Email FROM Representantes ORDER BY Nombre, IdRepresentante;";
        return conexion.Query<Representante>(sql).ToList();
    }

    public IReadOnlyList<Representante> ObtenerPagina(int pagina, int tamano)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "SELECT IdRepresentante, Nombre, Email FROM Representantes ORDER BY Nombre, IdRepresentante LIMIT @tamano OFFSET @offset;";
        return conexion.Query<Representante>(sql, new { tamano, offset = (pagina - 1) * tamano }).ToList();
    }

    public int Contar()
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Representantes;");
    }

    public Representante? Obtener(int id)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "SELECT IdRepresentante, Nombre, Email FROM Representantes WHERE IdRepresentante = @id;";
        return conexion.QuerySingleOrDefault<Representante>(sql, new { id });
    }

    public int Crear(Representante representante)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "INSERT INTO Representantes (Nombre, Email) VALUES (@Nombre, @Email);";
        conexion.Execute(sql, representante);
        return conexion.ExecuteScalar<int>("SELECT LAST_INSERT_ID();");
    }

    public void Actualizar(Representante representante)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = "UPDATE Representantes SET Nombre = @Nombre, Email = @Email WHERE IdRepresentante = @IdRepresentante;";
        conexion.Execute(sql, representante);
    }

    public int Eliminar(int id)
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.Execute("DELETE FROM Representantes WHERE IdRepresentante = @id;", new { id });
    }
}


