using System.Data;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace KameRequest.Repository;

// Centraliza la creacion de conexiones para que las pantallas no conozcan MySqlConnection.
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

// crea conexiones MySql para que los repositorios las usen
public class MySqlConnectionFactory(IConfiguration configuracion) : IDbConnectionFactory
{
    // crea una conexion nueva usando la cadena configurada
    public IDbConnection CreateConnection()
    {
        var cadena = configuracion.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("Falta la cadena de conexion MySql");

        return new MySqlConnection(cadena);
    }
}

