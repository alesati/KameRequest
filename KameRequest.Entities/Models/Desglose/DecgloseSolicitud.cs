namespace KameRequest.Models
{
    public class DesgloseSolicitud
    {
        public int IdDesglose { get; set; }
        public int IdSolicitud { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; } = DateTime.Now.Year;
        public decimal Horas { get; set; }
        public decimal Monto { get; set; }
        // IVA = 13% del Monto
        public decimal Iva => Monto * 0.13m;
        // Total = Monto + IVA
        public decimal Total => Monto + Iva;
        public decimal PorcentajeCobro { get; set; } = 100;
        public string? Observaciones { get; set; }
    }
}

