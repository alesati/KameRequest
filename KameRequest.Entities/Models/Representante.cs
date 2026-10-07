namespace KameRequest.Models;
// Representa los datos que SOL1 necesita para elegir un representante.
public class Representante
{
    public int IdRepresentante { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
