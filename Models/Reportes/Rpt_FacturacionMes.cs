namespace KameRequest.Models;

public class Rpt_FacturacionMes
{
    public string ConsecutivoOficio { get; set; }
    public string Descripcion { get; set; }
    public int Mes { get; set; }
    public int Anio { get; set; }
    public decimal Monto { get; set; }
    public decimal Iva { get; set; }  //calculos se hacen en la consulta a bd

    public decimal Total { get; set; } //calculos se hacen en la consulta a bd

}