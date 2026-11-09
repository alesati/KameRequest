namespace KameRequest.WebService.Modelos;

// Una linea de costo propuesta por el proveedor
public class DesglosePropuestaDto
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public decimal Horas { get; set; }
    public decimal Monto { get; set; }
    public decimal PorcentajeCobro { get; set; }
    public string Observaciones { get; set; } = string.Empty;

    // Estos valores se calculan para evitar guardar montos duplicados
    public decimal Iva => Math.Round(Monto * 0.13m, 2);
    public decimal Total => Monto + Iva;
}
