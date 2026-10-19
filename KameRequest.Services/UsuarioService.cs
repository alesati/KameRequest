using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using KameRequest.Services.Abstract;
using MySqlConnector;

namespace KameRequest.Services;

// resultado que indica por que un usuario pudo o no pudo iniciar sesion
public enum ResultadoAcceso
{
    Correcto,
    CredencialesInvalidas,
    UsuarioInactivo,
    UsuarioBloqueado
}

// contiene las reglas de login, bloqueo y manejo de contraseñas
public class UsuarioService(
    IConfiguration configuracion,
    UsuarioRepository usuarios) : IUsuarioService
{
    // crea el administrador solo si la base de datos aun no tiene usuarios
    public void CrearAdministradorInicialSiHaceFalta()
    {
        if (usuarios.Contar() > 0)
        {
            return;
        }

        var cifrado = Cifrar(
            configuracion["UsuarioInicial:Contrasena"] ?? "Admin123*");

        usuarios.CrearAdministrador(
            configuracion["UsuarioInicial:Nombre"] ?? "Administrador",
            configuracion["UsuarioInicial:Usuario"] ?? "admin",
            configuracion["UsuarioInicial:Correo"] ?? "admin@kamerequest.local",
            cifrado.ContrasenaCifrada,
            cifrado.Nonce,
            cifrado.EtiquetaAutenticacion);
    }

    // revisa credenciales, estado e intentos antes de permitir el acceso
    public ResultadoAcceso ValidarAcceso(InicioSesion datos, out Usuario? usuario)
    {
        usuario = usuarios.ObtenerPorNombre(datos.NombreUsuario);

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

        // compara la contraseña digitada con la que esta cifrada en la base
        if (Descifrar(usuario) == datos.Contrasena)
        {
            usuarios.ActualizarIntentos(usuario.IdUsuario, 0, "Activo");
            return ResultadoAcceso.Correcto;
        }

        // despues de tres intentos incorrectos se bloquea el usuario
        var intentos = usuario.IntentosFallidos + 1;
        var estado = intentos >= 3 ? "Bloqueado" : "Activo";
        usuarios.ActualizarIntentos(usuario.IdUsuario, intentos, estado);

        usuario = null;
        return ResultadoAcceso.CredencialesInvalidas;
    }

    public int ContarUsuarios()
    {
        return usuarios.Contar();
    }

    // recibe pagina y tamaño para no cargar todos los usuarios a la vez
    public IEnumerable<Usuario> ListarUsuarios(int pagina, int tamanoPagina)
    {
        return usuarios.Listar(pagina, tamanoPagina);
    }

    public IEnumerable<Usuario> ListarUsuarios()
    {
        return usuarios.Listar();
    }

    public UsuarioFormulario? ObtenerUsuario(int idUsuario)
    {
        return usuarios.ObtenerFormulario(idUsuario);
    }

    // valida que la contraseña y los datos unicos sean correctos antes de guardar
    public bool CrearUsuario(UsuarioFormulario datos, out string? error)
    {
        // los usuarios creados desde mantenimiento usan el tipo normal del sistema
        datos.TipoUsuario = "Usuario";

        if (string.IsNullOrWhiteSpace(datos.Contrasena))
        {
            error = "Digite una contraseña";
            return false;
        }

        if (usuarios.ExisteDuplicado(datos, false))
        {
            error = "El nombre de usuario o correo ya existe";
            return false;
        }

        var cifrado = Cifrar(datos.Contrasena);
        datos.IdUsuario = usuarios.Crear(
            datos,
            cifrado.ContrasenaCifrada,
            cifrado.Nonce,
            cifrado.EtiquetaAutenticacion);

        error = null;
        return true;
    }

    // actualiza los datos del usuario y cifra de nuevo solo si se cambio la contraseña
    public bool ActualizarUsuario(UsuarioFormulario datos, out string? error)
    {
        var usuarioActual = usuarios.ObtenerFormulario(datos.IdUsuario);
        if (usuarioActual is null)
        {
            error = "No se encontro el usuario";
            return false;
        }

        // el tipo se conserva internamente y no se modifica desde la pantalla
        datos.TipoUsuario = usuarioActual.TipoUsuario;

        if (usuarios.ExisteDuplicado(datos, true))
        {
            error = "El nombre de usuario o correo ya existe";
            return false;
        }

        if (string.IsNullOrWhiteSpace(datos.Contrasena))
        {
            usuarios.Actualizar(datos);
        }
        else
        {
            var cifrado = Cifrar(datos.Contrasena);
            usuarios.Actualizar(
                datos,
                cifrado.ContrasenaCifrada,
                cifrado.Nonce,
                cifrado.EtiquetaAutenticacion);
        }

        error = null;
        return true;
    }

    // valida el estado antes de actualizarlo en la base
    public bool CambiarEstadoUsuario(int idUsuario, string estado, out string? error)
    {
        var estadosPermitidos = new[] { "Activo", "Inactivo", "Bloqueado" };
        if (!estadosPermitidos.Contains(estado))
        {
            error = "El estado seleccionado no es valido";
            return false;
        }

        var actualizados = usuarios.CambiarEstado(idUsuario, estado);
        error = actualizados == 0 ? "No se encontro el usuario" : null;
        return actualizados > 0;
    }

    // captura la llave foranea para dar un mensaje claro al usuario
    public bool EliminarUsuario(int idUsuario, out string? error)
    {
        try
        {
            var eliminados = usuarios.Eliminar(idUsuario);
            error = eliminados == 0 ? "No se encontro el usuario" : null;
            return eliminados > 0;
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            error = "No se puede eliminar un registro con datos relacionados.";
            return false;
        }
    }

    // cifra una contraseña con AES-GCM antes de guardarla en la base de datos
    private (string ContrasenaCifrada, byte[] Nonce, byte[] EtiquetaAutenticacion) Cifrar(
        string contrasena)
    {
        var clave = Convert.FromBase64String(
            configuracion["Seguridad:ClaveAes"]
                ?? throw new InvalidOperationException("Falta configurar Seguridad:ClaveAes"));
        var nonce = RandomNumberGenerator.GetBytes(12);
        var texto = Encoding.UTF8.GetBytes(contrasena);
        var cifrado = new byte[texto.Length];
        var etiqueta = new byte[16];

        using var aes = new AesGcm(clave, etiqueta.Length);
        aes.Encrypt(nonce, texto, cifrado, etiqueta);

        return (Convert.ToBase64String(cifrado), nonce, etiqueta);
    }

    // descifra la contraseña para compararla durante el inicio de sesion
    private string Descifrar(Usuario usuario)
    {
        var clave = Convert.FromBase64String(
            configuracion["Seguridad:ClaveAes"]
                ?? throw new InvalidOperationException("Falta configurar Seguridad:ClaveAes"));
        var cifrado = Convert.FromBase64String(usuario.ContrasenaCifrada);
        var texto = new byte[cifrado.Length];

        using var aes = new AesGcm(clave, usuario.EtiquetaAutenticacion.Length);
        aes.Decrypt(usuario.Nonce, cifrado, usuario.EtiquetaAutenticacion, texto);

        return Encoding.UTF8.GetString(texto);
    }
}
