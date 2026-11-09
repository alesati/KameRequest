namespace KameRequest.WebService.Modelos;

// Informacion que recibe el servicio al confirmar una participacion
public class ParticipacionSolicitudDto
{
    public ProveedorDto Proveedor { get; set; } = new();
    public List<DesglosePropuestaDto> Desgloses { get; set; } = new();
}
