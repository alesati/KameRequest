using MySqlConnector;

namespace KameRequest.WebService.AccesoDatos.Infraestructura;

// Crea conexiones nuevas para cada operacion contra MariaDB
public class ConexionMariaDb(string cadenaConexion)
{
    public MySqlConnection CrearConexion() => new(cadenaConexion);
}
