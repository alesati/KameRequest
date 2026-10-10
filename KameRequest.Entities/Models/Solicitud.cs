using System.ComponentModel.DataAnnotations;

namespace KameRequest.Models;
// Representa una solicitud y los datos que se muestran de sus relaciones.
public class Solicitud
{
    public int IdSolicitud { get; set; }

    [Required(ErrorMessage = "El consecutivo de oficio es obligatorio.")]
    public string ConsecutivoOficio { get; set; } = string.Empty;
    // Se conserva este nombre para las vistas del modulo de tareas de Luis
    public string Consecutivo { get => ConsecutivoOficio; set => ConsecutivoOficio = value; }
    public string? DocumentoRespuesta { get; set; }
    public string? DocumentoInicio { get; set; }

    [Required(ErrorMessage = "El titulo de la solicitud es obligatorio.")]
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? IdRepresentante { get; set; }
    public string? Observaciones { get; set; }
    public int IdEstado { get; set; }

    [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime FechaIngreso { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime? FechaRespuesta { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FechaInicio { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Representante { get; set; }
    public string? CorreoRepresentante { get; set; }
}

