using System.ComponentModel.DataAnnotations;

namespace KameRequest.Models
{
    public class TareaSolicitud
    {
        public int IdTarea { get; set; }
        public int IdSolicitud { get; set; }
        public int IdUsuario { get; set; }
        public int? IdDesglose { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Usuario { get; set; } = string.Empty;
        public decimal HorasInvertidas { get; set; }
    }
}
