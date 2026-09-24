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

    // Crea una cuenta inicial solamente cuando la tabla aun no tiene usuarios
    public void CrearAdministradorInicialSiHaceFalta()
    {
        using var conexion = CrearConexion();
        if (conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Usuarios") > 0)
        {
            return;
        }

        var contrasena = configuracion["UsuarioInicial:Contrasena"] ?? "Admin123*";
        var cifrado = Cifrar(contrasena);
        conexion.Execute("""
            INSERT INTO Usuarios
                (NombreCompleto, NombreUsuario, Correo, TipoUsuario, Estado,
                 ContrasenaCifrada, Nonce, EtiquetaAutenticacion, IntentosFallidos)
            VALUES
                (@NombreCompleto, @NombreUsuario, @Correo, 'Administrador', 'Activo',
                 @ContrasenaCifrada, @Nonce, @EtiquetaAutenticacion, 0)
            """, new
        {
            NombreCompleto = configuracion["UsuarioInicial:Nombre"] ?? "Administrador",
            NombreUsuario = configuracion["UsuarioInicial:Usuario"] ?? "admin",
            Correo = configuracion["UsuarioInicial:Correo"] ?? "admin@kamerequest.local",
            cifrado.ContrasenaCifrada,
            cifrado.Nonce,
            cifrado.EtiquetaAutenticacion
        });
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

    // Cifra la contraseña antes de guardarla en la base de datos
    private (string ContrasenaCifrada, string Nonce, string EtiquetaAutenticacion) Cifrar(string contrasena)
    {
        var claveTexto = configuracion["Seguridad:ClaveAes"]
            ?? throw new InvalidOperationException("Falta configurar Seguridad:ClaveAes");
        var clave = Convert.FromBase64String(claveTexto);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var texto = Encoding.UTF8.GetBytes(contrasena);
        var cifrado = new byte[texto.Length];
        var etiqueta = new byte[16];

        using var aes = new AesGcm(clave, etiqueta.Length);
        aes.Encrypt(nonce, texto, cifrado, etiqueta);
        return (Convert.ToBase64String(cifrado), Convert.ToBase64String(nonce), Convert.ToBase64String(etiqueta));
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


