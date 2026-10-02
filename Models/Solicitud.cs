using System.ComponentModel.DataAnnotations;

namespace KameRequest.Models;

// Datos usados para mostrar una solicitud y sus relaciones.
public class Solicitud
{
    public int IdSolicitud { get; set; }
    public string Consecutivo { get; set; } = string.Empty;
    public string? DocumentoRespuesta { get; set; }
    public string? DocumentoInicio { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int? IdRepresentante { get; set; }
    public string Representante { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public int IdEstado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaRespuesta { get; set; }
}
