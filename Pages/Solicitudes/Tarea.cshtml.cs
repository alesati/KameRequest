using System.Security.Claims;
using Dapper;
using KameRequest.Models;
using KameRequest.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class TareasModel(IConfiguration configuracion, BitacoraRegistro bitacora) : PageModel
{
    public Solicitud? SolicitudActual { get; private set; }
    public List<TareaSolicitud> ListaTareas { get; private set; } = [];
    public decimal TotalHoras => ListaTareas.Sum(t => t.HorasInvertidas);

    [BindProperty]
    public TareaSolicitud NuevaTarea { get; set; } = new();

    [BindProperty]
    public TareaSolicitud TareaEditar { get; set; } = new();

    [TempData]
    public string? MensajeError { get; set; }

    public IActionResult OnGet(int id)
    {
        return CargarDatos(id) ? Page() : RedirectToPage("/Solicitudes/Index");
    }

    public IActionResult OnPostAgregar(int idSolicitud)
    {
        if (!ObtenerIdUsuario(out var idUsuario))
        {
            MensajeError = "No se pudo identificar el usuario de la sesion.";
            return RedirectToPage(new { id = idSolicitud });
        }

        if (string.IsNullOrWhiteSpace(NuevaTarea.Descripcion) || NuevaTarea.HorasInvertidas < 0)
        {
            MensajeError = "Digite la descripcion y las horas invertidas.";
            return RedirectToPage(new { id = idSolicitud });
        }

        using var conexion = CrearConexion();
        NuevaTarea.IdSolicitud = idSolicitud;
        NuevaTarea.IdUsuario = idUsuario;
        NuevaTarea.Usuario = User.Identity?.Name ?? "Usuario";
        conexion.Execute("""
            INSERT INTO Tareas (IdSolicitud, IdUsuario, Descripcion, Fecha, HorasInvertidas)
            VALUES (@IdSolicitud, @IdUsuario, @Descripcion, @Fecha, @HorasInvertidas);
            """, NuevaTarea);

        NuevaTarea.IdTarea = conexion.ExecuteScalar<int>("SELECT LAST_INSERT_ID();");
        bitacora.RegistrarCreacion(User, "tarea", NuevaTarea);
        return RedirectToPage(new { id = idSolicitud });
    }

    public IActionResult OnPostEditar(int idSolicitud)
    {
        using var conexion = CrearConexion();
        var anterior = ObtenerTarea(conexion, TareaEditar.IdTarea);
        if (anterior is null || anterior.IdSolicitud != idSolicitud)
        {
            MensajeError = "No se encontro la tarea solicitada.";
            return RedirectToPage(new { id = idSolicitud });
        }

        if (string.IsNullOrWhiteSpace(TareaEditar.Descripcion) || TareaEditar.HorasInvertidas < 0)
        {
            MensajeError = "Digite la descripcion y las horas invertidas.";
            return RedirectToPage(new { id = idSolicitud });
        }

        conexion.Execute("""
            UPDATE Tareas
            SET Descripcion = @Descripcion, Fecha = @Fecha, HorasInvertidas = @HorasInvertidas
            WHERE IdTarea = @IdTarea;
            """, TareaEditar);

        var actual = ObtenerTarea(conexion, TareaEditar.IdTarea);
        bitacora.RegistrarActualizacion(User, "tarea", anterior, actual);
        return RedirectToPage(new { id = idSolicitud });
    }

    public IActionResult OnPostEliminar(int idTarea, int idSolicitud)
    {
        using var conexion = CrearConexion();
        var tarea = ObtenerTarea(conexion, idTarea);
        if (tarea is null || tarea.IdSolicitud != idSolicitud)
        {
            return RedirectToPage(new { id = idSolicitud });
        }

        // Una tarea ligada a un desglose tiene datos relacionados y no se elimina
        if (tarea.IdDesglose is not null)
        {
            MensajeError = "No se puede eliminar un registro con datos relacionados.";
            return RedirectToPage(new { id = idSolicitud });
        }

        conexion.Execute("DELETE FROM Tareas WHERE IdTarea = @idTarea", new { idTarea });
        bitacora.RegistrarEliminacion(User, "tarea", tarea);
        return RedirectToPage(new { id = idSolicitud });
    }

    private bool CargarDatos(int idSolicitud)
    {
        using var conexion = CrearConexion();
        SolicitudActual = conexion.QuerySingleOrDefault<Solicitud>("""
            SELECT s.IdSolicitud, s.ConsecutivoOficio, s.Titulo, s.Descripcion,
                   s.IdRepresentante, s.IdEstado, s.FechaIngreso, s.FechaRespuesta, s.FechaInicio,
                   COALESCE(r.Nombre, 'Sin asignar') AS Representante,
                   COALESCE(e.Nombre, 'Sin estado') AS Estado
            FROM Solicitudes s
            LEFT JOIN Representantes r ON r.IdRepresentante = s.IdRepresentante
            LEFT JOIN EstadosSolicitud e ON e.IdEstado = s.IdEstado
            WHERE s.IdSolicitud = @idSolicitud;
            """, new { idSolicitud });

        if (SolicitudActual is null)
        {
            return false;
        }

        ListaTareas = conexion.Query<TareaSolicitud>("""
            SELECT t.IdTarea, t.IdSolicitud, t.IdUsuario, t.IdDesglose, t.Descripcion,
                   t.Fecha, t.HorasInvertidas, u.NombreCompleto AS Usuario
            FROM Tareas t
            INNER JOIN Usuarios u ON u.IdUsuario = t.IdUsuario
            WHERE t.IdSolicitud = @idSolicitud
            ORDER BY t.Fecha DESC, t.IdTarea DESC;
            """, new { idSolicitud }).ToList();
        return true;
    }

    private static TareaSolicitud? ObtenerTarea(MySqlConnection conexion, int idTarea) =>
        conexion.QuerySingleOrDefault<TareaSolicitud>("""
            SELECT t.IdTarea, t.IdSolicitud, t.IdUsuario, t.IdDesglose, t.Descripcion,
                   t.Fecha, t.HorasInvertidas, u.NombreCompleto AS Usuario
            FROM Tareas t
            INNER JOIN Usuarios u ON u.IdUsuario = t.IdUsuario
            WHERE t.IdTarea = @idTarea;
            """, new { idTarea });

    private bool ObtenerIdUsuario(out int idUsuario) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out idUsuario);

    private MySqlConnection CrearConexion() =>
        new(configuracion.GetConnectionString("MySql"));
}
