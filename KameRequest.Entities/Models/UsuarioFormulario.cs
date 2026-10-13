using System.ComponentModel.DataAnnotations;

namespace KameRequest.Models;
// Datos que se usan al crear o editar un usuario
public class UsuarioFormulario
{
   
    public int IdUsuario { get; set; } // El id se usa al editar, al crear queda en cero
    [Required(ErrorMessage = "Digite el nombre completo")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Digite el nombre de usuario")]
    [Display(Name = "Nombre de usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    // EmailAddress revisa que el formato del correo sea valido
    [Required(ErrorMessage = "Digite el correo")]
    [EmailAddress(ErrorMessage = "Digite un correo valido")]
    public string Correo { get; set; } = string.Empty;

    // Los usuarios nuevos quedan con este tipo por defecto
    [Required]
    [Display(Name = "Tipo de usuario")]
    public string TipoUsuario { get; set; } = "Usuario";

    // Estado controla si el usuario puede iniciar sesion
    [Required]
    public string Estado { get; set; } = "Activo";

    // Password oculta los caracteres en la pantalla
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string? Contrasena { get; set; }
}
