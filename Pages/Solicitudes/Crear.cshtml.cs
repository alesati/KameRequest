using KameRequest.Services;
using KameRequest.Services.Abstract;
// logica de la pantalla Crear.cshtml
using KameRequest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class CrearModel(ISolicitudService solicitudes, IRepresentanteService representantes, IBitacoraService bitacora) : PageModel
{
    [BindProperty]
    public Solicitud Solicitud { get; set; } = new();
    public IReadOnlyList<Representante> Representantes { get; private set; } = [];

    // carga los datos necesarios antes de mostrar la pantalla
    public void OnGet()
    {
        Solicitud.FechaIngreso = DateTime.Today;
        CargarRepresentantes();
    }

    // procesa los datos enviados desde el formulario
    public IActionResult OnPost()
    {
        CargarRepresentantes();
        if (!ModelState.IsValid)
            return Page();
        try
        {
            Solicitud.IdSolicitud = solicitudes.Crear(Solicitud);
        }
        catch (ArgumentException error)
        {
            ModelState.AddModelError("Solicitud.ConsecutivoOficio", error.Message);
            return Page();
        }

        var creada = solicitudes.Obtener(Solicitud.IdSolicitud) ?? Solicitud;
        bitacora.RegistrarCreacion(User, "solicitud", creada);
        TempData["Mensaje"] = "La solicitud se creo correctamente.";
        return RedirectToPage("Index");
    }

    private void CargarRepresentantes() => Representantes = representantes.Listar();
}

