using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class EditarModel(SolicitudRepository solicitudes, RepresentanteRepository representantes, BitacoraRegistro bitacora) : PageModel
{
    [BindProperty]
    public Solicitud Solicitud { get; set; } = new();
    public IReadOnlyList<Representante> Representantes { get; private set; } = [];
    public IReadOnlyList<EstadoSolicitud> Estados { get; private set; } = [];

    public IActionResult OnGet(int id)
    {
        Solicitud = solicitudes.Obtener(id) ?? new();
        CargarCatalogos();
        return Solicitud.IdSolicitud == 0 ? NotFound() : Page();
    }

    public IActionResult OnPost()
    {
        CargarCatalogos();
        if (!ModelState.IsValid)
            return Page();
        var anterior = solicitudes.Obtener(Solicitud.IdSolicitud);
        if (anterior is null)
            return NotFound();
        solicitudes.Actualizar(Solicitud);
        bitacora.RegistrarActualizacion(User, "solicitud", anterior, solicitudes.Obtener(Solicitud.IdSolicitud));
        TempData["Mensaje"] = "La solicitud se actualizo correctamente.";
        return RedirectToPage("Index");
    }

    private void CargarCatalogos()
    {
        Representantes = representantes.Listar();
        Estados = solicitudes.ListarEstados();
    }
}
