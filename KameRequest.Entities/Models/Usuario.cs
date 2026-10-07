namespace KameRequest.Models;
// Representa los datos necesarios para validar una cuenta del sistema
public class Usuario
{
    // Datos que se leen desde la tabla Usuarios
    // Identificador unico de la cuenta en la tabla Usuarios
    public int IdUsuario { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string TipoUsuario { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    // Estos tres valores se ocupan para recuperar la contraseña de forma segura
    public string ContrasenaCifrada { get; set; } = string.Empty;
    public byte[] Nonce { get; set; } = [];
    public byte[] EtiquetaAutenticacion { get; set; } = [];
    // Cuenta los errores de contraseña para bloquear despues del limite
    public int IntentosFallidos { get; set; }
}
