using Dapper;
using KameRequest.Models;
using KameRequest.Clases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySqlConnector;

namespace KameRequest.Pages.Reportes
{
    public class FacturacionMesModel(IConfiguration configuration, BitacoraRegistro bitacora) : PageModel
    {
        
        [BindProperty(SupportsGet = true)]
        public int Mes { get; set; } = DateTime.Now.Month;

        [BindProperty(SupportsGet = true)]
        public int Anio { get; set; } = DateTime.Now.Year;

        public List<Rpt_FacturacionMes> ListaReporte { get; set; } = new List<Rpt_FacturacionMes>();
        public decimal Subtotal => ListaReporte.Sum(t => t.Monto);
        public decimal TotalIva => ListaReporte.Sum(t => t.Iva);
        public decimal Total => ListaReporte.Sum(t => t.Total);
        public bool BusquedaRealizada { get; set; } = false;

        public void OnGet()
        {
           

            // Si el usuario envió parámetros por GET, ejecutamos la búsqueda
            if (Mes > 0 && Anio > 0)
            {
                BusquedaRealizada = true;
                CargarReporte();
                bitacora.RegistrarConsulta(User, "reporte de tareas por usuario");
            }
        }

      

        private void CargarReporte()
        {
            var conexionString = configuration.GetConnectionString("MySql");

            using (var conexion = new MySqlConnection(conexionString))
            {
                string sql = @"
                    SELECT 
                        t0.ConsecutivoOficio,
                        t0.Descripcion,
                        t1.Mes,
                        t1.Anio,
                        t1.Monto,
                        t1.Monto * 0.13 as IVA,
                        (t1.Monto * 0.13)+t1.Monto as Total
                
                    FROM solicitudes T0
                    INNER JOIN desglosessolicitud t1 ON t0.IdSolicitud = t1.IdSolicitud
                    
                    WHERE T1.Mes = @Mes 
                      AND T1.Anio = @Anio
                    ORDER BY 1 ASC";

                ListaReporte = conexion.Query<Rpt_FacturacionMes>(sql, new
                {
                    Mes = Mes,
                    Anio = Anio
                }).ToList();
            }
        }
    }
}

