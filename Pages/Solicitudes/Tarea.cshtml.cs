// logica de la pantalla Tarea.cshtml
using System.Security.Claims;
using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class TareasModel(SolicitudRepository solicitudes, TareaRepository tareas, BitacoraRegistro bitacora) : PageModel
{
    private const int RegistrosPorPagina = 5;
    public Solicitud? SolicitudActual { get; private set; }
    public List<TareaSolicitud> ListaTareas { get; private set; } = [];
    public int PaginaActual { get; private set; }
    public int TotalPaginas { get; private set; }
    public decimal TotalHoras => ListaTareas.Sum(t => t.HorasInvertidas);

    [BindProperty]
    public TareaSolicitud NuevaTarea { get; set; } = new();

    [BindProperty]
    public TareaSolicitud TareaEditar { get; set; } = new();

    [TempData]
    public string? MensajeError { get; set; }

    // carga los datos necesarios antes de mostrar la pantalla
    public IActionResult OnGet(int id, int pagina = 1) => CargarDatos(id, pagina) ? Page() : RedirectToPage("/Solicitudes/Index");
    // valida y agrega el nuevo dato relacionado
    public IActionResult OnPostAgregar(int idSolicitud)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario))
        {
            MensajeError = "No se pudo identificar el usuario de la sesion.";
            return RedirectToPage(new { id = idSolicitud });
        }

        if (string.IsNullOrWhiteSpace(NuevaTarea.Descripcion) || NuevaTarea.HorasInvertidas < 0)
        {
            MensajeError = "Digite la descripcion y las horas invertidas.";
            return RedirectToPage(new { id = idSolicitud });
        }

        NuevaTarea.IdSolicitud = idSolicitud;
        NuevaTarea.IdUsuario = idUsuario;
        NuevaTarea.Usuario = User.Identity?.Name ?? "Usuario";
        NuevaTarea.IdTarea = tareas.Crear(NuevaTarea);
        bitacora.RegistrarCreacion(User, "tarea", NuevaTarea);
        return RedirectToPage(new { id = idSolicitud });
    }

    // valida y guarda los cambios del formulario
    public IActionResult OnPostEditar(int idSolicitud)
    {
        var anterior = tareas.Obtener(TareaEditar.IdTarea);
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

        tareas.Actualizar(TareaEditar);
        bitacora.RegistrarActualizacion(User, "tarea", anterior, tareas.Obtener(TareaEditar.IdTarea));
        return RedirectToPage(new { id = idSolicitud });
    }

    // confirma y procesa la eliminacion del registro
    public IActionResult OnPostEliminar(int idTarea, int idSolicitud)
    {
        var tarea = tareas.Obtener(idTarea);
        if (tarea is null || tarea.IdSolicitud != idSolicitud)
            return RedirectToPage(new { id = idSolicitud });
        if (tarea.IdDesglose is not null)
        {
            MensajeError = "No se puede eliminar un registro con datos relacionados.";
            return RedirectToPage(new { id = idSolicitud });
        }

        tareas.Eliminar(idTarea);
        bitacora.RegistrarEliminacion(User, "tarea", tarea);
        return RedirectToPage(new { id = idSolicitud });
    }

    private bool CargarDatos(int idSolicitud, int pagina)
    {
        SolicitudActual = solicitudes.Obtener(idSolicitud);
        if (SolicitudActual is null)
            return false;

        var total = tareas.ContarPorSolicitud(idSolicitud);
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)RegistrosPorPagina));
        PaginaActual = Math.Clamp(pagina, 1, TotalPaginas);
        ListaTareas = tareas.ObtenerPaginaPorSolicitud(idSolicitud, PaginaActual, RegistrosPorPagina).ToList();
        return true;
    }
}

