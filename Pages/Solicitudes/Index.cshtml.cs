using Dapper;
using KameRequest.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Solicitudes
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<Solicitud> ListaSolicitudes { get; set; } = new List<Solicitud>();

        public void OnGet()
        {
            var conexionString = _configuration.GetConnectionString("MySql");

            using (var conexion = new MySqlConnection(conexionString))
            {
                // TODO BITACORA: Registrar la consulta de la lista de solicitudes

                string sql = @"
                    SELECT 
                        s.IdSolicitud,
                        s.ConsecutivoOficio,
                        s.Titulo,
                        s.Descripcion,
                        s.DocumentoRespuesta,
                        s.DocumentoInicio,
                        COALESCE(r.Nombre, 'Sin asignar') AS Representante,
                        COALESCE(e.Nombre, 'Sin estado') AS Estado,
                        s.FechaIngreso,
                        s.FechaRespuesta,
                        s.FechaInicio,
                        s.Observaciones
                    FROM Solicitudes s
                    LEFT JOIN Representantes r ON s.IdRepresentante = r.IdRepresentante
                    LEFT JOIN EstadosSolicitud e ON s.IdEstado = e.IdEstado
                    ORDER BY s.FechaIngreso DESC";

                ListaSolicitudes = conexion.Query<Solicitud>(sql).ToList();
            }
        }
    }
}