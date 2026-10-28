namespace KameRequest.Models;

// Linea de una propuesta enviada desde el portal externo.
public class DesgloseParticipacion
{
    public int IdDesgloseParticipacion { get; set; }
    public int IdParticipacion { get; set; }
    public int Mes { get; set; }
    public int Anio { get; set; }
    public decimal Horas { get; set; }
    public decimal Monto { get; set; }
    public decimal PorcentajeCobro { get; set; }
    public string Observaciones { get; set; } = string.Empty;
    public decimal Iva => Math.Round(Monto * 0.13m, 2);
    public decimal Total => Monto + Iva;
}