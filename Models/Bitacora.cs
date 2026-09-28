namespace KameRequest.Models;

// Una accion realizada dentro del sistema.
public class Bitacora
{
    public DateTime Fecha { get; set; }
    public string Usuario { get; set; } = "Sistema";
    public string Descripcion { get; set; } = string.Empty;
}
