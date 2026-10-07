namespace KameRequest.Models;
// Accion registrada para auditoria del sistema.
public class Bitacora
{
    public DateTime Fecha { get; set; }
    public string Usuario { get; set; } = "Sistema";
    public string Descripcion { get; set; } = string.Empty;
}
