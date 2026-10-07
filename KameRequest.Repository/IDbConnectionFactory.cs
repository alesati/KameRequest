using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace KameRequest.Repository;

// Centraliza la creacion de conexiones para que las pantallas no conozcan SqlConnection.
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqlServerConnectionFactory(IConfiguration configuracion) : IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        var cadena = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("Falta la cadena de conexion SqlServer");

        return new SqlConnection(cadena);
    }
}
