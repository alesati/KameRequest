namespace KameRequest.WebService.Modelos;

// Solicitud donde un proveedor ya registro una propuesta
public class ParticipacionProveedorDto
{
    public int IdParticipacion { get; set; }
    public string ConsecutivoOficio { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public DateTime FechaParticipacion { get; set; }
    public string Estado { get; set; } = string.Empty;
}
