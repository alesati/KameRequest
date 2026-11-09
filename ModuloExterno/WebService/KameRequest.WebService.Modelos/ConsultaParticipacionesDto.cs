namespace KameRequest.WebService.Modelos;

// respuesta que devuelve la consulta de participaciones en el web service
public class ConsultaParticipacionesDto
{
    public ResultadoOperacionDto Resultado { get; set; } = new();
    public List<ParticipacionProveedorDto> Participaciones { get; set; } = new();
}
