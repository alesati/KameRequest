using Dapper;
using KameRequest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Solicitudes
{
    public class DesgloseModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public DesgloseModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Solicitud? SolicitudActual { get; set; }
        public List<DesgloseSolicitud> ListaDesgloses { get; set; } = new List<DesgloseSolicitud>();

        public decimal SumaMontos => ListaDesgloses.Sum(d => d.Monto);
        public decimal SumaIva => ListaDesgloses.Sum(d => d.Iva);
        public decimal TotalGeneral => ListaDesgloses.Sum(d => d.Total);

        [BindProperty]
        public DesgloseSolicitud NuevoDesglose { get; set; } = new DesgloseSolicitud();

        [BindProperty]
        public DesgloseSolicitud DesgloseEditar { get; set; } = new DesgloseSolicitud();

        public IActionResult OnGet(int id)
        {
            if (id <= 0) return RedirectToPage("/Solicitudes/Index");

            var conexionString = _configuration.GetConnectionString("MySql");

            using (var conexion = new MySqlConnection(conexionString))
            {
                // Cargar Encabezado
                string sqlSolicitud = @"
                    SELECT 
                        s.IdSolicitud,
                        s.ConsecutivoOficio,
                        s.Titulo,
                        s.Descripcion,
                        COALESCE(r.Nombre, 'Sin asignar') AS Representante,
                        COALESCE(e.Nombre, 'Sin estado') AS Estado,
                        s.FechaIngreso,
                        s.FechaRespuesta,
                        s.FechaInicio
                    FROM Solicitudes s
                    LEFT JOIN Representantes r ON s.IdRepresentante = r.IdRepresentante
                    LEFT JOIN EstadosSolicitud e ON s.IdEstado = e.IdEstado
                    WHERE s.IdSolicitud = @Id";

                SolicitudActual = conexion.QueryFirstOrDefault<Solicitud>(sqlSolicitud, new { Id = id });

                if (SolicitudActual == null) return RedirectToPage("/Solicitudes/Index");

                // Cargar Desgloses
                string sqlDesgloses = @"
                    SELECT IdDesglose, IdSolicitud, Mes, Anio, Horas, Monto, Iva, Total, PorcentajeCobro, Observaciones 
                    FROM DesglosesSolicitud 
                    WHERE IdSolicitud = @Id";

                ListaDesgloses = conexion.Query<DesgloseSolicitud>(sqlDesgloses, new { Id = id }).ToList();
            }

            return Page();
        }

        public IActionResult OnPostAgregar(int idSolicitud)
        {
            var conexionString = _configuration.GetConnectionString("MySql");

            decimal ivaCalculado = NuevoDesglose.Monto * 0.13m;
            decimal totalCalculado = NuevoDesglose.Monto + ivaCalculado;

            using (var conexion = new MySqlConnection(conexionString))
            {
                string sqlInsert = @"
                    INSERT INTO DesglosesSolicitud 
                    (IdSolicitud, Mes, Anio, Horas, Monto, Iva, Total, PorcentajeCobro, Observaciones)
                    VALUES 
                    (@IdSolicitud, @Mes, @Anio, @Horas, @Monto, @Iva, @Total, @PorcentajeCobro, @Observaciones)";

                conexion.Execute(sqlInsert, new
                {
                    IdSolicitud = idSolicitud,
                    Mes = NuevoDesglose.Mes,
                    Anio = NuevoDesglose.Anio,
                    Horas = NuevoDesglose.Horas,
                    Monto = NuevoDesglose.Monto,
                    Iva = ivaCalculado,
                    Total = totalCalculado,
                    PorcentajeCobro = NuevoDesglose.PorcentajeCobro,
                    Observaciones = NuevoDesglose.Observaciones
                });

                // TODO BITACORA: Registrar inserción de desglose
            }

            return RedirectToPage(new { id = idSolicitud });
        }

        // POST: Editar Desglose Existente
        public IActionResult OnPostEditar(int idSolicitud)
        {
            var conexionString = _configuration.GetConnectionString("MySql");

            decimal ivaCalculado = DesgloseEditar.Monto * 0.13m;
            decimal totalCalculado = DesgloseEditar.Monto + ivaCalculado;

            using (var conexion = new MySqlConnection(conexionString))
            {
                string sqlUpdate = @"
                    UPDATE DesglosesSolicitud 
                    SET Mes = @Mes, 
                        Anio = @Anio, 
                        Horas = @Horas, 
                        Monto = @Monto, 
                        Iva = @Iva, 
                        Total = @Total, 
                        PorcentajeCobro = @PorcentajeCobro, 
                        Observaciones = @Observaciones
                    WHERE IdDesglose = @IdDesglose";

                conexion.Execute(sqlUpdate, new
                {
                    IdDesglose = DesgloseEditar.IdDesglose,
                    Mes = DesgloseEditar.Mes,
                    Anio = DesgloseEditar.Anio,
                    Horas = DesgloseEditar.Horas,
                    Monto = DesgloseEditar.Monto,
                    Iva = ivaCalculado,
                    Total = totalCalculado,
                    PorcentajeCobro = DesgloseEditar.PorcentajeCobro,
                    Observaciones = DesgloseEditar.Observaciones
                });

                // TODO BITACORA: Registrar actualización de desglose
            }

            return RedirectToPage(new { id = idSolicitud });
        }

        public IActionResult OnPostEliminar(int idDesglose, int idSolicitud)
        {
            var conexionString = _configuration.GetConnectionString("MySql");

            using (var conexion = new MySqlConnection(conexionString))
            {
                string sqlDelete = "DELETE FROM DesglosesSolicitud WHERE IdDesglose = @Id";
                conexion.Execute(sqlDelete, new { Id = idDesglose });

                // TODO BITACORA: Registrar eliminación de desglose
            }

            return RedirectToPage(new { id = idSolicitud });
        }
    }
}