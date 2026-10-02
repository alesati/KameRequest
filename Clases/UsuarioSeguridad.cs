using System.Security.Cryptography;
using System.Text;
using Dapper;
using KameRequest.Models;
using MySqlConnector;

namespace KameRequest.Clases;

public enum ResultadoAcceso
{
    // Posibles resultados que devuelve el intento de login
    Correcto,
    CredencialesInvalidas,
    UsuarioInactivo,
    UsuarioBloqueado
}

// Consulta el usuario y valida la contraseña antes de crear la sesion
public class UsuarioSeguridad(IConfiguration configuracion)
{
    // Lee la conexion configurada y crea una conexion nueva con MySql
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
        // Busca solamente la cuenta que tiene el usuario escrito en el login
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

        // No permite ingresar si el administrador bloqueo la cuenta
        if (usuario.Estado == "Bloqueado")
        {
            return ResultadoAcceso.UsuarioBloqueado;
        }

        // Tampoco permite ingresar cuando el usuario esta inactivo
        if (usuario.Estado != "Activo")
        {
            return ResultadoAcceso.UsuarioInactivo;
        }

        if (Descifrar(usuario) == datos.Contrasena)
        {
            // Al ingresar bien se reinician los intentos fallidos anteriores
            conexion.Execute("UPDATE Usuarios SET IntentosFallidos = 0 WHERE IdUsuario = @IdUsuario", usuario);
            return ResultadoAcceso.Correcto;
        }

        var intentos = usuario.IntentosFallidos + 1;
        // Despues de 3 fallos se deja el usuario bloqueado
        var estado = intentos >= 3 ? "Bloqueado" : "Activo";
        conexion.Execute("""
            UPDATE Usuarios
            SET IntentosFallidos = @intentos, Estado = @estado
            WHERE IdUsuario = @IdUsuario
            """, new { intentos, estado, usuario.IdUsuario });

        usuario = null;
        return ResultadoAcceso.CredencialesInvalidas;
    }

    public int ContarUsuarios()
    {
        using var conexion = CrearConexion();
        // Este total se usa para calcular cuantos botones de pagina se muestran
        return conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Usuarios");
    }

    public IEnumerable<Usuario> ListarUsuarios(int pagina, int tamanoPagina)
    {
        using var conexion = CrearConexion();
        // LIMIT muestra solo una parte de la lista y OFFSET indica desde donde inicia
        return conexion.Query<Usuario>("""
            SELECT IdUsuario, NombreCompleto, NombreUsuario, Correo, TipoUsuario, Estado, IntentosFallidos
            FROM Usuarios
            ORDER BY NombreCompleto
            LIMIT @tamanoPagina OFFSET @desplazamiento
            """, new { tamanoPagina, desplazamiento = (pagina - 1) * tamanoPagina });
    }
    public IEnumerable<Usuario> ListarUsuarios()
    {
        using var conexion = CrearConexion();
        // Esta version devuelve todos los usuarios cuando otra pantalla lo necesita
        return conexion.Query<Usuario>("""
            SELECT IdUsuario, NombreCompleto, NombreUsuario, Correo, TipoUsuario, Estado, IntentosFallidos
            FROM Usuarios
            ORDER BY NombreCompleto
            """);
    }

    public UsuarioFormulario? ObtenerUsuario(int idUsuario)
    {
        using var conexion = CrearConexion();
        // Busca un solo usuario para cargar sus datos en el formulario de editar
        return conexion.QuerySingleOrDefault<UsuarioFormulario>("""
            SELECT IdUsuario, NombreCompleto, NombreUsuario, Correo, TipoUsuario, Estado
            FROM Usuarios WHERE IdUsuario = @idUsuario
            """, new { idUsuario });
    }

    public bool CrearUsuario(UsuarioFormulario datos, out string? error)
    {
        // La contraseña es obligatoria solamente cuando se crea una cuenta
        if (string.IsNullOrWhiteSpace(datos.Contrasena))
        {
            error = "Digite una contraseña";
            return false;
        }

        using var conexion = CrearConexion();
        // Evita que se repita un usuario o correo que ya esta registrado
        var existe = conexion.ExecuteScalar<int>("""
            SELECT COUNT(*) FROM Usuarios
            WHERE NombreUsuario = @NombreUsuario OR Correo = @Correo
            """, datos) > 0;
        if (existe)
        {
            error = "El nombre de usuario o correo ya existe";
            return false;
        }

        var cifrado = Cifrar(datos.Contrasena);
        // Guarda la contraseña cifrada, nunca la contraseña original
        conexion.Execute("""
            INSERT INTO Usuarios (NombreCompleto, NombreUsuario, Correo, TipoUsuario, Estado,
                ContrasenaCifrada, Nonce, EtiquetaAutenticacion, IntentosFallidos)
            VALUES (@NombreCompleto, @NombreUsuario, @Correo, @TipoUsuario, @Estado,
                @ContrasenaCifrada, @Nonce, @EtiquetaAutenticacion, 0)
            """, new { datos.NombreCompleto, datos.NombreUsuario, datos.Correo, datos.TipoUsuario, datos.Estado,
                cifrado.ContrasenaCifrada, cifrado.Nonce, cifrado.EtiquetaAutenticacion });
        error = null;
        return true;
    }

    public bool ActualizarUsuario(UsuarioFormulario datos, out string? error)
    {
        using var conexion = CrearConexion();
        // Revisa repetidos sin comparar el mismo registro que se esta editando
        var existe = conexion.ExecuteScalar<int>("""
            SELECT COUNT(*) FROM Usuarios
            WHERE (NombreUsuario = @NombreUsuario OR Correo = @Correo) AND IdUsuario <> @IdUsuario
            """, datos) > 0;
        if (existe)
        {
            error = "El nombre de usuario o correo ya existe";
            return false;
        }

        if (string.IsNullOrWhiteSpace(datos.Contrasena))
        {
            // Si se deja vacia conserva la contraseña que ya tenia el usuario
            conexion.Execute("""
                UPDATE Usuarios SET NombreCompleto = @NombreCompleto, NombreUsuario = @NombreUsuario,
                Correo = @Correo, TipoUsuario = @TipoUsuario, Estado = @Estado
                WHERE IdUsuario = @IdUsuario
                """, datos);
        }
        else
        {
            // Si se escribe una nueva se cifra antes de actualizarla
            var cifrado = Cifrar(datos.Contrasena);
            conexion.Execute("""
                UPDATE Usuarios SET NombreCompleto = @NombreCompleto, NombreUsuario = @NombreUsuario,
                Correo = @Correo, TipoUsuario = @TipoUsuario, Estado = @Estado,
                ContrasenaCifrada = @ContrasenaCifrada, Nonce = @Nonce, EtiquetaAutenticacion = @EtiquetaAutenticacion
                WHERE IdUsuario = @IdUsuario
                """, new { datos.NombreCompleto, datos.NombreUsuario, datos.Correo, datos.TipoUsuario, datos.Estado,
                    datos.IdUsuario, cifrado.ContrasenaCifrada, cifrado.Nonce, cifrado.EtiquetaAutenticacion });
        }
        error = null;
        return true;
    }
    public bool CambiarEstadoUsuario(int idUsuario, string estado, out string? error)
    {
        // Solo permite los estados definidos para las cuentas
        var estadosValidos = new[] { "Activo", "Inactivo", "Bloqueado" };
        if (!estadosValidos.Contains(estado))
        {
            error = "El estado seleccionado no es valido";
            return false;
        }

        using var conexion = CrearConexion();
        // Cuando vuelve a Activo tambien borra los intentos fallidos del login
        var actualizados = conexion.Execute("""
            UPDATE Usuarios
            SET Estado = @estado,
                IntentosFallidos = IF(@estado = 'Activo', 0, IntentosFallidos)
            WHERE IdUsuario = @idUsuario
            """, new { idUsuario, estado });

        error = actualizados == 0 ? "No se encontro el usuario" : null;
        return actualizados > 0;
    }

    public bool EliminarUsuario(int idUsuario, out string? error)
    {
        using var conexion = CrearConexion();
        try
        {
            // DELETE devuelve la cantidad de filas que realmente se eliminaron
            var eliminados = conexion.Execute("DELETE FROM Usuarios WHERE IdUsuario = @idUsuario", new { idUsuario });
            error = eliminados == 0 ? "No se encontro el usuario" : null;
            return eliminados > 0;
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            // MySql devuelve 1451 cuando otra tabla tiene datos relacionados
            error = "No se puede eliminar un registro con datos relacionados.";
            return false;
        }
    }
    // Cifra la contraseña antes de guardarla en la base de datos
    private (string ContrasenaCifrada, string Nonce, string EtiquetaAutenticacion) Cifrar(string contrasena)
    {
        var claveTexto = configuracion["Seguridad:ClaveAes"]
            ?? throw new InvalidOperationException("Falta configurar Seguridad:ClaveAes");
        // Convierte la clave guardada como texto base64 a bytes para AES
        var clave = Convert.FromBase64String(claveTexto);
        // El nonce cambia en cada cifrado para que no se repita el resultado
        var nonce = RandomNumberGenerator.GetBytes(12);
        var texto = Encoding.UTF8.GetBytes(contrasena);
        var cifrado = new byte[texto.Length];
        var etiqueta = new byte[16];

        using var aes = new AesGcm(clave, etiqueta.Length);
        // AES GCM genera el texto cifrado y una etiqueta para validar los datos
        aes.Encrypt(nonce, texto, cifrado, etiqueta);
        return (Convert.ToBase64String(cifrado), Convert.ToBase64String(nonce), Convert.ToBase64String(etiqueta));
    }
    // La clave se guarda en configuracion y debe tener 32 bytes para AES GCM 256
    private string Descifrar(Usuario usuario)
    {
        var claveTexto = configuracion["Seguridad:ClaveAes"]
            ?? throw new InvalidOperationException("Falta configurar Seguridad:ClaveAes");
        var clave = Convert.FromBase64String(claveTexto);
        // Convierte los valores que vienen de la base otra vez a bytes
        var cifrado = Convert.FromBase64String(usuario.ContrasenaCifrada);
        var nonce = Convert.FromBase64String(usuario.Nonce);
        var etiqueta = Convert.FromBase64String(usuario.EtiquetaAutenticacion);
        var textoPlano = new byte[cifrado.Length];

        using var aes = new AesGcm(clave, etiqueta.Length);
        // Recupera el texto para poder compararlo con la contraseña digitada
        aes.Decrypt(nonce, cifrado, etiqueta, textoPlano);
        return Encoding.UTF8.GetString(textoPlano);
    }
}






