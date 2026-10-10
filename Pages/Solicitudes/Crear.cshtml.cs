// logica de la pantalla Crear.cshtml
using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class CrearModel(SolicitudRepository solicitudes, RepresentanteRepository representantes, BitacoraRegistro bitacora) : PageModel
{
    [BindProperty]
    public Solicitud Solicitud { get; set; } = new();
    public IReadOnlyList<Representante> Representantes { get; private set; } = [];

    public void OnGet()
    {
        Solicitud.FechaIngreso = DateTime.Today;
        CargarRepresentantes();
    }

    public IActionResult OnPost()
    {
        CargarRepresentantes();
        if (!ModelState.IsValid)
            return Page();
        Solicitud.IdSolicitud = solicitudes.Crear(Solicitud);
        var creada = solicitudes.Obtener(Solicitud.IdSolicitud) ?? Solicitud;
        bitacora.RegistrarCreacion(User, "solicitud", creada);
        TempData["Mensaje"] = "La solicitud se creo correctamente.";
        return RedirectToPage("Index");
    }

    private void CargarRepresentantes() => Representantes = representantes.Listar();
}

