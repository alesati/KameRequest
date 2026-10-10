namespace KameRequest.Models;

// datos que muestra el reporte mensual de facturacion
public class Rpt_FacturacionMes
{
    public string ConsecutivoOficio { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Mes { get; set; }
    public int Anio { get; set; }
    public decimal Monto { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
}
