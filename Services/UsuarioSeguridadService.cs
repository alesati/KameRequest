using System.Security.Cryptography;
using System.Text;
using Dapper;
using KameRequest.Models;
using MySqlConnector;

namespace KameRequest.Services;

public enum ResultadoAcceso
{
    Correcto,
    CredencialesInvalidas,
    UsuarioInactivo,
    UsuarioBloqueado
}

// Consulta el usuario y valida la contraseña antes de crear la sesion
public class UsuarioSeguridadService(IConfiguration configuracion)
{
    private MySqlConnection CrearConexion()
    {
        var cadena = configuracion.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("Falta la cadena de conexion MySql");
        return new MySqlConnection(cadena);
    }

    public ResultadoAcceso ValidarAcceso(InicioSesion datos, out Usuario? usuario)
    {
        using var conexion = CrearConexion();
        usuario = conexion.QuerySingleOrDefault<Usuario>("""
            SELECT IdUsuario, NombreCompleto, NombreUsuario, Correo, TipoUsuario, Estado,
                   ContrasenaCifrada, Nonce, EtiquetaAutenticacion, IntentosFallidos
            FROM Usuarios
            WHERE NombreUsuario = @NombreUsuario
            """, new { datos.NombreUsuario });

        if (usuario is null)
        {
            return ResultadoAcceso.CredencialesInvalidas;
        }

        if (usuario.Estado == "Bloqueado")
        {
            return ResultadoAcceso.UsuarioBloqueado;
        }

        if (usuario.Estado != "Activo")
        {
            return ResultadoAcceso.UsuarioInactivo;
        }

        if (Descifrar(usuario) == datos.Contrasena)
        {
            conexion.Execute("UPDATE Usuarios SET IntentosFallidos = 0 WHERE IdUsuario = @IdUsuario", usuario);
            return ResultadoAcceso.Correcto;
        }

        var intentos = usuario.IntentosFallidos + 1;
        var estado = intentos >= 3 ? "Bloqueado" : "Activo";
        conexion.Execute("""
            UPDATE Usuarios
            SET IntentosFallidos = @intentos, Estado = @estado
            WHERE IdUsuario = @IdUsuario
            """, new { intentos, estado, usuario.IdUsuario });

        usuario = null;
        return ResultadoAcceso.CredencialesInvalidas;
    }

    // La clave se guarda en configuracion y debe tener 32 bytes para AES GCM 256
    private string Descifrar(Usuario usuario)
    {
        var claveTexto = configuracion["Seguridad:ClaveAes"]
            ?? throw new InvalidOperationException("Falta configurar Seguridad:ClaveAes");
        var clave = Convert.FromBase64String(claveTexto);
        var cifrado = Convert.FromBase64String(usuario.ContrasenaCifrada);
        var nonce = Convert.FromBase64String(usuario.Nonce);
        var etiqueta = Convert.FromBase64String(usuario.EtiquetaAutenticacion);
        var textoPlano = new byte[cifrado.Length];

        using var aes = new AesGcm(clave, etiqueta.Length);
        aes.Decrypt(nonce, cifrado, etiqueta, textoPlano);
        return Encoding.UTF8.GetString(textoPlano);
    }
}
