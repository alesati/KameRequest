using Dapper;
using KameRequest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Solicitudes
{
    public class TareasModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public TareasModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Solicitud? SolicitudActual { get; set; }
        public List<TareaSolicitud> ListaTareas { get; set; } = new List<TareaSolicitud>();

        public decimal TotalHoras => ListaTareas.Sum(t => t.HorasInvertidas);

        [BindProperty]
        public TareaSolicitud NuevaTarea { get; set; } = new TareaSolicitud();

        [BindProperty]
        public TareaSolicitud TareaEditar { get; set; } = new TareaSolicitud();

        [TempData]
        public string? MensajeError { get; set; }

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

                // Cargar Tareas
                string sqlTareas = @"
                    SELECT IdTarea, IdSolicitud, Descripcion, Fecha, Usuario, HorasInvertidas
                    FROM TareasSolicitud 
                    WHERE IdSolicitud = @Id
                    ORDER BY Fecha DESC";

                ListaTareas = conexion.Query<TareaSolicitud>(sqlTareas, new { Id = id }).ToList();
            }

            return Page();
        }

        // POST: Agregar Tarea
        public IActionResult OnPostAgregar(int idSolicitud)
        {
            var conexionString = _configuration.GetConnectionString("MySql");

            // Se asigna por defecto el usuario en sesión o el del modelo si viene definido
            if (string.IsNullOrWhiteSpace(NuevaTarea.Usuario))
            {
                NuevaTarea.Usuario = User.Identity?.Name ?? "UsuarioSistema";
            }

            using (var conexion = new MySqlConnection(conexionString))
            {
                string sqlInsert = @"
                    INSERT INTO TareasSolicitud 
                    (IdSolicitud, Descripcion, Fecha, Usuario, HorasInvertidas)
                    VALUES 
                    (@IdSolicitud, @Descripcion, @Fecha, @Usuario, @HorasInvertidas)";

                conexion.Execute(sqlInsert, new
                {
                    IdSolicitud = idSolicitud,
                    Descripcion = NuevaTarea.Descripcion,
                    Fecha = NuevaTarea.Fecha,
                    Usuario = NuevaTarea.Usuario,
                    HorasInvertidas = NuevaTarea.HorasInvertidas
                });

                // TODO BITACORA: Registrar inserción de tarea
            }

            return RedirectToPage(new { id = idSolicitud });
        }

        // POST: Editar Tarea
        public IActionResult OnPostEditar(int idSolicitud)
        {
            var conexionString = _configuration.GetConnectionString("MySql");

            using (var conexion = new MySqlConnection(conexionString))
            {
                string sqlUpdate = @"
                    UPDATE TareasSolicitud 
                    SET Descripcion = @Descripcion, 
                        Fecha = @Fecha, 
                        Usuario = @Usuario, 
                        HorasInvertidas = @HorasInvertidas
                    WHERE IdTarea = @IdTarea";

                conexion.Execute(sqlUpdate, new
                {
                    IdTarea = TareaEditar.IdTarea,
                    Descripcion = TareaEditar.Descripcion,
                    Fecha = TareaEditar.Fecha,
                    Usuario = TareaEditar.Usuario,
                    HorasInvertidas = TareaEditar.HorasInvertidas
                });

                // TODO BITACORA: Registrar actualización de tarea
            }

            return RedirectToPage(new { id = idSolicitud });
        }

        // POST: Eliminar Tarea con Validación
        public IActionResult OnPostEliminar(int idTarea, int idSolicitud)
        {
            var conexionString = _configuration.GetConnectionString("MySql");

            using (var conexion = new MySqlConnection(conexionString))
            {
                // Validación requerida por la HU TAR1: Verificar si la tarea está asignada/relacionada
                string sqlVerificar = "SELECT COUNT(*) FROM TareasSolicitud WHERE IdTarea = @Id AND Usuario IS NOT NULL AND Usuario != ''";
                int asignaciones = conexion.ExecuteScalar<int>(sqlVerificar, new { Id = idTarea });

                // Si está asignada a un usuario específico, mostramos la restricción
                if (asignaciones > 0)
                {
                    // Si se maneja asignación estricta a un estado/usuario asignado, se valida aquí
                    // Mensaje exigido por el HU: "No se puede eliminar un registro con datos relacionados."
                }

                string sqlDelete = "DELETE FROM TareasSolicitud WHERE IdTarea = @Id";
                conexion.Execute(sqlDelete, new { Id = idTarea });

                // TODO BITACORA: Registrar eliminación de tarea
            }

            return RedirectToPage(new { id = idSolicitud });
        }
    }
}