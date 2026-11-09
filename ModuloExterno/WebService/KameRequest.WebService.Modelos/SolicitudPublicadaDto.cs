namespace KameRequest.WebService.Modelos;

// Datos basicos que se muestran en el listado publico de solicitudes
public class SolicitudPublicadaDto
{
    public int IdSolicitud { get; set; }
    public string ConsecutivoOficio { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string? Representante { get; set; }
    public DateTime FechaVencimientoPublicacion { get; set; }
}
