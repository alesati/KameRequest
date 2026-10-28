namespace KameRequest.Models;

// Propuesta enviada por un proveedor para una solicitud publicada.
public class ParticipacionSolicitud
{
    public int IdParticipacion { get; set; }
    public int IdSolicitud { get; set; }
    public int IdProveedor { get; set; }
    public string CedulaJuridica { get; set; } = string.Empty;
    public string NombreProveedor { get; set; } = string.Empty;
    public string NombreRepresentante { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public DateTime FechaParticipacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public IReadOnlyList<DesgloseParticipacion> Desgloses { get; set; } = [];
}