using KameRequest.Services;
using KameRequest.Services.Abstract;
// logica de la pantalla Desglose.cshtml
using KameRequest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class DesgloseModel(ISolicitudService solicitudes, IDesgloseService desgloses, IBitacoraService bitacora) : PageModel
{
    private const int RegistrosPorPagina = 5;
    public Solicitud? SolicitudActual { get; private set; }
    public List<DesgloseSolicitud> ListaDesgloses { get; private set; } = [];
    public int PaginaActual { get; private set; }
    public int TotalPaginas { get; private set; }
    public decimal SumaMontos => ListaDesgloses.Sum(d => d.Monto);
    public decimal SumaIva => ListaDesgloses.Sum(d => d.Iva);
    public decimal TotalGeneral => ListaDesgloses.Sum(d => d.Total);

    [BindProperty]
    public DesgloseSolicitud NuevoDesglose { get; set; } = new();

    [BindProperty]
    public DesgloseSolicitud DesgloseEditar { get; set; } = new();

    [TempData]
    public string? MensajeError { get; set; }

    // carga los datos necesarios antes de mostrar la pantalla
    public IActionResult OnGet(int id, int pagina = 1) => CargarDatos(id, pagina) ? Page() : RedirectToPage("/Solicitudes/Index");
    // valida y agrega el nuevo dato relacionado
    public IActionResult OnPostAgregar(int idSolicitud, bool modal = false)
    {
        try
        {
            NuevoDesglose.IdDesglose = desgloses.CrearParaSolicitud(idSolicitud, NuevoDesglose);
            bitacora.RegistrarCreacion(User, "desglose", NuevoDesglose);
        }
        catch (ArgumentException error)
        {
            MensajeError = error.Message;
            return VolverAListado(idSolicitud, modal);
        }

        TempData["Mensaje"] = "El desglose se creo correctamente.";
        return VolverAListado(idSolicitud, modal);
    }

    // valida y guarda los cambios del formulario
    public IActionResult OnPostEditar(int idSolicitud, bool modal = false)
    {
        try
        {
            var anterior = desgloses.ActualizarParaSolicitud(idSolicitud, DesgloseEditar);
            bitacora.RegistrarActualizacion(User, "desglose", anterior, desgloses.Obtener(DesgloseEditar.IdDesglose));
        }
        catch (ArgumentException error)
        {
            MensajeError = error.Message;
            return VolverAListado(idSolicitud, modal);
        }
        TempData["Mensaje"] = "El desglose se actualizo correctamente.";
        return VolverAListado(idSolicitud, modal);
    }

    // confirma y procesa la eliminacion del registro
    public IActionResult OnPostEliminar(int idDesglose, int idSolicitud, bool modal = false)
    {
        try
        {
            var eliminado = desgloses.EliminarDeSolicitud(idSolicitud, idDesglose);
            if (eliminado is not null)
            {
                bitacora.RegistrarEliminacion(User, "desglose", eliminado);
                TempData["Mensaje"] = "El desglose se elimino correctamente.";
            }
        }
        catch (InvalidOperationException error)
        {
            MensajeError = error.Message;
        }

        return VolverAListado(idSolicitud, modal);
    }

    // conserva la vista reducida cuando la accion viene desde el modal de solicitudes
    private RedirectToPageResult VolverAListado(int idSolicitud, bool modal) =>
        RedirectToPage(new { id = idSolicitud, modal = modal ? 1 : (int?)null });

    private bool CargarDatos(int idSolicitud, int pagina)
    {
        SolicitudActual = solicitudes.Obtener(idSolicitud);
        if (SolicitudActual is null)
            return false;

        var total = desgloses.ContarPorSolicitud(idSolicitud);
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)RegistrosPorPagina));
        PaginaActual = Math.Clamp(pagina, 1, TotalPaginas);
        ListaDesgloses = desgloses.ObtenerPaginaPorSolicitud(idSolicitud, PaginaActual, RegistrosPorPagina).ToList();
        return true;
    }

}
