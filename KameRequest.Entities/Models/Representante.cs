using System.ComponentModel.DataAnnotations;

namespace KameRequest.Models;
// Representa los datos que SOL1 necesita para elegir un representante.
public class Representante
{
    public int IdRepresentante { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Digite un correo valido.")]
    public string Email { get; set; } = string.Empty;
}

