using Dapper;
using KameRequest.Models;

namespace KameRequest.Repository.Repositories;

public class BitacoraRepository(IDbConnectionFactory conexiones)
{
    public void Registrar(int? idUsuario, string descripcion)
    {
        using var conexion = conexiones.CreateConnection();
        conexion.Execute("INSERT INTO Bitacoras (Fecha, IdUsuario, Descripcion) VALUES (NOW(), @idUsuario, @descripcion);", new { idUsuario, descripcion });
    }

    public int Contar()
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Bitacoras");
    }

    public IReadOnlyList<Bitacora> ObtenerPagina(int pagina, int registrosPorPagina)
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.Query<Bitacora>("""
            SELECT b.Fecha, COALESCE(u.NombreCompleto, 'Sistema') AS Usuario, b.Descripcion
            FROM Bitacoras b LEFT JOIN Usuarios u ON u.IdUsuario = b.IdUsuario
            ORDER BY b.Fecha DESC
            LIMIT @registrosPorPagina OFFSET @offset;
            """, new { registrosPorPagina, offset = (pagina - 1) * registrosPorPagina }).ToList();
    }
}


