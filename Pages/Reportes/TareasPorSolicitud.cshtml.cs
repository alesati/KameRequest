using Dapper;
using KameRequest.Models;
using KameRequest.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySqlConnector;

namespace KameRequest.Pages.Reportes
{
    public class TareasPorSolicitudModel(IConfiguration configuration, BitacoraService bitacora) : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int IdSolicitudSeleccionada { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Mes { get; set; } = DateTime.Now.Month;

        [BindProperty(SupportsGet = true)]
        public int Anio { get; set; } = DateTime.Now.Year;

        public List<SelectListItem> ListaSolicitudes { get; set; } = new List<SelectListItem>();
        public List<TareaReporteDto> ListaReporte { get; set; } = new List<TareaReporteDto>();
        public decimal TotalHoras => ListaReporte.Sum(t => t.HorasInvertidas);
        public bool BusquedaRealizada { get; set; } = false;
        public string InfoSolicitud { get; set; } = string.Empty;

        public void OnGet()
        {
            CargarSolicitudes();

            // Si se envió una solicitud válida por GET, ejecutamos la búsqueda
            if (IdSolicitudSeleccionada > 0 && Mes > 0 && Anio > 0)
            {
                BusquedaRealizada = true;
                CargarReporte();
                bitacora.RegistrarConsulta(User, "reporte de tareas por solicitud");
            }
        }

        private void CargarSolicitudes()
        {
            var conexionString = configuration.GetConnectionString("MySql");

            using (var conexion = new MySqlConnection(conexionString))
            {
                // Carga todas las solicitudes para el combo box
                string sql = "SELECT IdSolicitud, ConsecutivoOficio, Titulo FROM Solicitudes ORDER BY IdSolicitud DESC";
                var solicitudes = conexion.Query(sql);

                ListaSolicitudes = solicitudes.Select(s => new SelectListItem
                {
                    Value = s.IdSolicitud.ToString(),
                    Text = $"{s.ConsecutivoOficio} - {s.Titulo}",
                    Selected = (s.IdSolicitud == IdSolicitudSeleccionada)
                }).ToList();
            }
        }

        private void CargarReporte()
        {
            var conexionString = configuration.GetConnectionString("MySql");

            using (var conexion = new MySqlConnection(conexionString))
            {
                string sql = @"
                    SELECT 
                        u.NombreCompleto AS Usuario,
                        t.Fecha,
                        t.Descripcion,
                        t.HorasInvertidas,
                        s.ConsecutivoOficio,
                        s.Titulo AS TituloSolicitud
                    FROM Tareas t
                    INNER JOIN Usuarios u ON u.IdUsuario = t.IdUsuario
                    INNER JOIN Solicitudes s ON t.IdSolicitud = s.IdSolicitud
                    WHERE t.IdSolicitud = @IdSolicitud 
                      AND MONTH(t.Fecha) = @Mes 
                      AND YEAR(t.Fecha) = @Anio
                    ORDER BY t.Fecha ASC";

                ListaReporte = conexion.Query<TareaReporteDto>(sql, new
                {
                    IdSolicitud = IdSolicitudSeleccionada,
                    Mes = Mes,
                    Anio = Anio
                }).ToList();

                // Si hay resultados tomamos el título/oficio de los datos; de lo contrario, del combo box
                if (ListaReporte.Any())
                {
                    InfoSolicitud = $"{ListaReporte.First().ConsecutivoOficio} - {ListaReporte.First().TituloSolicitud}";
                }
                else
                {
                    var seleccion = ListaSolicitudes.FirstOrDefault(x => x.Value == IdSolicitudSeleccionada.ToString());
                    InfoSolicitud = seleccion != null ? seleccion.Text : "Solicitud Seleccionada";
                }
            }
        }
    }
}
