namespace KameRequest.WebService.Modelos;

// Informacion completa que WordPress muestra a un proveedor
public class SolicitudDetalleExternoDto
{
    public int IdSolicitud { get; set; }
    public string ConsecutivoOficio { get; set; } = string.Empty;
    public string? DocumentoRespuesta { get; set; }
    public string? DocumentoInicio { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Representante { get; set; }
    public string? Observaciones { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public DateTime? FechaRespuesta { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime FechaVencimientoPublicacion { get; set; }
}
