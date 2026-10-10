// datos que usa esta parte del sistema
namespace KameRequest.Models
{
    public class TareaReporteDto
    {
        public string Usuario { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal HorasInvertidas { get; set; }
        public string ConsecutivoOficio { get; set; } = string.Empty;
        public string TituloSolicitud { get; set; } = string.Empty;
    }
}

