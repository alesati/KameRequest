using System.ComponentModel.DataAnnotations;

namespace KameRequest.Models;
// Datos que se reciben en el formulario para iniciar sesion
public class InicioSesion
{
    // Los atributos validan los dos campos antes de intentar el login
    [Required(ErrorMessage = "Digite su nombre de usuario")]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Digite su contraseña")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;
}
