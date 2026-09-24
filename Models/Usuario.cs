namespace KameRequest.Models;

// Representa los datos necesarios para validar una cuenta del sistema
public class Usuario
{
    public int IdUsuario { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string TipoUsuario { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string ContrasenaCifrada { get; set; } = string.Empty;
    public string Nonce { get; set; } = string.Empty;
    public string EtiquetaAutenticacion { get; set; } = string.Empty;
    public int IntentosFallidos { get; set; }
}
