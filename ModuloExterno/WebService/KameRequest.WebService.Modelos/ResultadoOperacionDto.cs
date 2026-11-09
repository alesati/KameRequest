namespace KameRequest.WebService.Modelos;

// Respuesta comun para informar si una operacion fue exitosa
public class ResultadoOperacionDto
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public int? Id { get; set; }
}
