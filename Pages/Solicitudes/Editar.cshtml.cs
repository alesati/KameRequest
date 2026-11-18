using KameRequest.Services;
using KameRequest.Services.Abstract;
// logica de la pantalla Editar.cshtml
using KameRequest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class EditarModel(
    ISolicitudService solicitudes,
    IRepresentanteService representantes,
    IBitacoraService bitacora) : PageModel
{
    [BindProperty]
    public Solicitud Solicitud { get; set; } = new();
    public IReadOnlyList<Representante> Representantes { get; private set; } = [];
    public IReadOnlyList<EstadoSolicitud> Estados { get; private set; } = [];

    // carga los datos necesarios antes de mostrar la pantalla
    public IActionResult OnGet(int id)
    {
        Solicitud = solicitudes.Obtener(id) ?? new();
        CargarCatalogos();
        return Solicitud.IdSolicitud == 0 ? NotFound() : Page();
    }

    // procesa los datos enviados desde el formulario
    public IActionResult OnPost()
    {
        CargarCatalogos();
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var cambio = solicitudes.ActualizarConNotificacion(User, Solicitud);
            bitacora.RegistrarActualizacion(User, "solicitud", cambio.Anterior, cambio.Actual);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }

        TempData["Mensaje"] = "La solicitud se actualizo correctamente.";
        return RedirectToPage("/Solicitudes/Index");
    }

    private void CargarCatalogos()
    {
        Representantes = representantes.Listar();
        Estados = solicitudes.ListarEstados();
    }
}
