// consultas de base de datos para este modulo
using Dapper;
using KameRequest.Models;

namespace KameRequest.Repository.Repositories;

public class UsuarioRepository(IDbConnectionFactory conexiones)
{
    public int Contar()
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Usuarios;");
    }

    public Usuario? ObtenerPorNombre(string nombreUsuario)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT IdUsuario, NombreCompleto, NombreUsuario, Correo, TipoUsuario, Estado,
                   ContrasenaCifrada, Nonce, EtiquetaAutenticacion, IntentosFallidos
            FROM Usuarios
            WHERE NombreUsuario = @nombreUsuario;
            """;
        return conexion.QuerySingleOrDefault<Usuario>(sql, new { nombreUsuario });
    }

    public IEnumerable<Usuario> Listar(int? pagina = null, int? tamano = null)
    {
        using var conexion = conexiones.CreateConnection();
        if (pagina.HasValue && tamano.HasValue)
        {
            const string sqlPaginado = """
                SELECT IdUsuario, NombreCompleto, NombreUsuario, Correo, TipoUsuario, Estado, IntentosFallidos
                FROM Usuarios
                ORDER BY NombreCompleto
                LIMIT @tamano OFFSET @offset;
                """;
            return conexion.Query<Usuario>(sqlPaginado, new { tamano, offset = (pagina.Value - 1) * tamano.Value });
        }

        const string sql = """
            SELECT IdUsuario, NombreCompleto, NombreUsuario, Correo, TipoUsuario, Estado, IntentosFallidos
            FROM Usuarios
            ORDER BY NombreCompleto;
            """;
        return conexion.Query<Usuario>(sql);
    }

    public UsuarioFormulario? ObtenerFormulario(int idUsuario)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT IdUsuario, NombreCompleto, NombreUsuario, Correo, TipoUsuario, Estado
            FROM Usuarios
            WHERE IdUsuario = @idUsuario;
            """;
        return conexion.QuerySingleOrDefault<UsuarioFormulario>(sql, new { idUsuario });
    }

    public bool ExisteDuplicado(UsuarioFormulario datos, bool excluirActual)
    {
        using var conexion = conexiones.CreateConnection();
        var sql = excluirActual ? "SELECT COUNT(*) FROM Usuarios WHERE (NombreUsuario = @NombreUsuario OR Correo = @Correo) AND IdUsuario <> @IdUsuario;" : "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @NombreUsuario OR Correo = @Correo;";
        return conexion.ExecuteScalar<int>(sql, datos) > 0;
    }

    public int Crear(UsuarioFormulario datos, string cifrada, byte[] nonce, byte[] etiqueta)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            INSERT INTO Usuarios
                (NombreCompleto, NombreUsuario, Correo, TipoUsuario, Estado,
                 ContrasenaCifrada, Nonce, EtiquetaAutenticacion, IntentosFallidos)
            VALUES
                (@NombreCompleto, @NombreUsuario, @Correo, @TipoUsuario, @Estado,
                 @cifrada, @nonce, @etiqueta, 0);
            """;
        conexion.Execute(sql, new { datos.NombreCompleto, datos.NombreUsuario, datos.Correo, datos.TipoUsuario, datos.Estado, cifrada, nonce, etiqueta });
        return conexion.ExecuteScalar<int>("SELECT LAST_INSERT_ID();");
    }

    public void CrearAdministrador(string nombre, string usuario, string correo, string cifrada, byte[] nonce, byte[] etiqueta)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            INSERT INTO Usuarios
                (NombreCompleto, NombreUsuario, Correo, TipoUsuario, Estado,
                 ContrasenaCifrada, Nonce, EtiquetaAutenticacion, IntentosFallidos)
            VALUES
                (@nombre, @usuario, @correo, 'Administrador', 'Activo',
                 @cifrada, @nonce, @etiqueta, 0);
            """;
        conexion.Execute(sql, new { nombre, usuario, correo, cifrada, nonce, etiqueta });
    }

    public void Actualizar(UsuarioFormulario datos, string? cifrada = null, byte[]? nonce = null, byte[]? etiqueta = null)
    {
        using var conexion = conexiones.CreateConnection();
        if (cifrada is null)
        {
            const string sqlSinContrasena = """
                UPDATE Usuarios
                SET NombreCompleto = @NombreCompleto,
                    NombreUsuario = @NombreUsuario,
                    Correo = @Correo,
                    TipoUsuario = @TipoUsuario,
                    Estado = @Estado
                WHERE IdUsuario = @IdUsuario;
                """;
            conexion.Execute(sqlSinContrasena, datos);
            return;
        }

        const string sqlConContrasena = """
            UPDATE Usuarios
            SET NombreCompleto = @NombreCompleto,
                NombreUsuario = @NombreUsuario,
                Correo = @Correo,
                TipoUsuario = @TipoUsuario,
                Estado = @Estado,
                ContrasenaCifrada = @cifrada,
                Nonce = @nonce,
                EtiquetaAutenticacion = @etiqueta
            WHERE IdUsuario = @IdUsuario;
            """;
        conexion.Execute(sqlConContrasena, new { datos.NombreCompleto, datos.NombreUsuario, datos.Correo, datos.TipoUsuario, datos.Estado, datos.IdUsuario, cifrada, nonce, etiqueta });
    }

    public void ActualizarIntentos(int idUsuario, int intentos, string estado)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            UPDATE Usuarios
            SET IntentosFallidos = @intentos,
                Estado = @estado
            WHERE IdUsuario = @idUsuario;
            """;
        conexion.Execute(sql, new { idUsuario, intentos, estado });
    }

    public int CambiarEstado(int idUsuario, string estado)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            UPDATE Usuarios
            SET Estado = @estado,
                IntentosFallidos = CASE WHEN @estado = 'Activo' THEN 0 ELSE IntentosFallidos END
            WHERE IdUsuario = @idUsuario;
            """;
        return conexion.Execute(sql, new { idUsuario, estado });
    }

    public int Eliminar(int idUsuario)
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.Execute("DELETE FROM Usuarios WHERE IdUsuario = @idUsuario;", new { idUsuario });
    }
}


