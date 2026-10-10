using System.Security.Cryptography;
using System.Text;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using MySqlConnector;

namespace KameRequest.Clases;

public enum ResultadoAcceso
{
    Correcto,
    CredencialesInvalidas,
    UsuarioInactivo,
    UsuarioBloqueado
}

// Contiene reglas de seguridad; las consultas se realizan en UsuarioRepository.
// contiene las reglas para login, bloqueo y contraseñas
public class UsuarioSeguridad(IConfiguration configuracion, UsuarioRepository usuarios)
{
    public void CrearAdministradorInicialSiHaceFalta()
    {
        if (usuarios.Contar() > 0)
            return;
        var cifrado = Cifrar(configuracion["UsuarioInicial:Contrasena"] ?? "Admin123*");
        usuarios.CrearAdministrador(configuracion["UsuarioInicial:Nombre"] ?? "Administrador", configuracion["UsuarioInicial:Usuario"] ?? "admin", configuracion["UsuarioInicial:Correo"] ?? "admin@kamerequest.local", cifrado.ContrasenaCifrada, cifrado.Nonce, cifrado.EtiquetaAutenticacion);
    }

    public ResultadoAcceso ValidarAcceso(InicioSesion datos, out Usuario? usuario)
    {
        usuario = usuarios.ObtenerPorNombre(datos.NombreUsuario);
        if (usuario is null)
            return ResultadoAcceso.CredencialesInvalidas;
        if (usuario.Estado == "Bloqueado")
            return ResultadoAcceso.UsuarioBloqueado;
        if (usuario.Estado != "Activo")
            return ResultadoAcceso.UsuarioInactivo;
        if (Descifrar(usuario) == datos.Contrasena)
        {
            usuarios.ActualizarIntentos(usuario.IdUsuario, 0, "Activo");
            return ResultadoAcceso.Correcto;
        }

        var intentos = usuario.IntentosFallidos + 1;
        usuarios.ActualizarIntentos(usuario.IdUsuario, intentos, intentos >= 3 ? "Bloqueado" : "Activo");
        usuario = null;
        return ResultadoAcceso.CredencialesInvalidas;
    }

    public int ContarUsuarios() => usuarios.Contar();
    public IEnumerable<Usuario> ListarUsuarios(int pagina, int tamanoPagina) => usuarios.Listar(pagina, tamanoPagina);
    public IEnumerable<Usuario> ListarUsuarios() => usuarios.Listar();
    public UsuarioFormulario? ObtenerUsuario(int idUsuario) => usuarios.ObtenerFormulario(idUsuario);
    public bool CrearUsuario(UsuarioFormulario datos, out string? error)
    {
        // Los usuarios creados desde el mantenimiento usan el rol normal del sistema.
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
        datos.IdUsuario = usuarios.Crear(datos, cifrado.ContrasenaCifrada, cifrado.Nonce, cifrado.EtiquetaAutenticacion);
        error = null;
        return true;
    }

    public bool ActualizarUsuario(UsuarioFormulario datos, out string? error)
    {
        // El tipo se conserva internamente y no se modifica desde la pantalla.
        var usuarioActual = usuarios.ObtenerFormulario(datos.IdUsuario);
        if (usuarioActual is null)
        {
            error = "No se encontro el usuario";
            return false;
        }

        datos.TipoUsuario = usuarioActual.TipoUsuario;

        if (usuarios.ExisteDuplicado(datos, true))
        {
            error = "El nombre de usuario o correo ya existe";
            return false;
        }

        if (string.IsNullOrWhiteSpace(datos.Contrasena))
            usuarios.Actualizar(datos);
        else
        {
            var cifrado = Cifrar(datos.Contrasena);
            usuarios.Actualizar(datos, cifrado.ContrasenaCifrada, cifrado.Nonce, cifrado.EtiquetaAutenticacion);
        }

        error = null;
        return true;
    }

    public bool CambiarEstadoUsuario(int idUsuario, string estado, out string? error)
    {
        if (!new[]
        {
            "Activo",
            "Inactivo",
            "Bloqueado"
        }.Contains(estado))
        {
            error = "El estado seleccionado no es valido";
            return false;
        }

        var actualizados = usuarios.CambiarEstado(idUsuario, estado);
        error = actualizados == 0 ? "No se encontro el usuario" : null;
        return actualizados > 0;
    }

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

    private (string ContrasenaCifrada, byte[] Nonce, byte[] EtiquetaAutenticacion) Cifrar(string contrasena)
    {
        var clave = Convert.FromBase64String(configuracion["Seguridad:ClaveAes"] ?? throw new InvalidOperationException("Falta configurar Seguridad:ClaveAes"));
        var nonce = RandomNumberGenerator.GetBytes(12);
        var texto = Encoding.UTF8.GetBytes(contrasena);
        var cifrado = new byte[texto.Length];
        var etiqueta = new byte[16];
        using var aes = new AesGcm(clave, etiqueta.Length);
        aes.Encrypt(nonce, texto, cifrado, etiqueta);
        return (Convert.ToBase64String(cifrado), nonce, etiqueta);
    }

    private string Descifrar(Usuario usuario)
    {
        var clave = Convert.FromBase64String(configuracion["Seguridad:ClaveAes"] ?? throw new InvalidOperationException("Falta configurar Seguridad:ClaveAes"));
        var cifrado = Convert.FromBase64String(usuario.ContrasenaCifrada);
        var texto = new byte[cifrado.Length];
        using var aes = new AesGcm(clave, usuario.EtiquetaAutenticacion.Length);
        aes.Decrypt(usuario.Nonce, cifrado, usuario.EtiquetaAutenticacion, texto);
        return Encoding.UTF8.GetString(texto);
    }
}


