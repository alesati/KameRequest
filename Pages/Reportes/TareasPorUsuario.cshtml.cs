using Dapper;
using KameRequest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySqlConnector;

namespace KameRequest.Pages.Reportes
{
    public class TareasPorUsuarioModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public TareasPorUsuarioModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty(SupportsGet = true)]
        public string? UsuarioSeleccionado { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Mes { get; set; } = DateTime.Now.Month;

        [BindProperty(SupportsGet = true)]
        public int Anio { get; set; } = DateTime.Now.Year;

        public List<SelectListItem> ListaUsuarios { get; set; } = new List<SelectListItem>();
        public List<TareaReporteDto> ListaReporte { get; set; } = new List<TareaReporteDto>();
        public decimal TotalHoras => ListaReporte.Sum(t => t.HorasInvertidas);
        public bool BusquedaRealizada { get; set; } = false;

        public void OnGet()
        {
            CargarUsuariosConTareas();

            // Si el usuario envió parámetros por GET, ejecutamos la búsqueda
            if (!string.IsNullOrWhiteSpace(UsuarioSeleccionado) && Mes > 0 && Anio > 0)
            {
                BusquedaRealizada = true;
                CargarReporte();
            }
        }

        private void CargarUsuariosConTareas()
        {
            var conexionString = _configuration.GetConnectionString("MySql");

            using (var conexion = new MySqlConnection(conexionString))
            {
                // Carga solo los usuarios que tienen tareas registradas
                string sql = """
                    SELECT DISTINCT u.NombreCompleto
                    FROM Tareas t
                    INNER JOIN Usuarios u ON u.IdUsuario = t.IdUsuario
                    ORDER BY u.NombreCompleto;
                    """;
                var usuarios = conexion.Query<string>(sql).ToList();

                ListaUsuarios = usuarios.Select(u => new SelectListItem
                {
                    Value = u,
                    Text = u,
                    Selected = (u == UsuarioSeleccionado)
                }).ToList();
            }
        }

        private void CargarReporte()
        {
            var conexionString = _configuration.GetConnectionString("MySql");

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
                    WHERE u.NombreCompleto = @Usuario
                      AND MONTH(t.Fecha) = @Mes 
                      AND YEAR(t.Fecha) = @Anio
                    ORDER BY t.Fecha ASC";

                ListaReporte = conexion.Query<TareaReporteDto>(sql, new
                {
                    Usuario = UsuarioSeleccionado,
                    Mes = Mes,
                    Anio = Anio
                }).ToList();
            }
        }
    }
}
