// logica de la pantalla Crear.cshtml
using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class CrearModel(SolicitudRepository solicitudes, RepresentanteRepository representantes, BitacoraRegistro bitacora) : PageModel
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
        catch (MySqlException error) when (error.Number == 1062)
        {
            // evita mostrar el error de MariaDB cuando el consecutivo ya existe
            ModelState.AddModelError("Solicitud.ConsecutivoOficio", "El consecutivo de oficio ya existe.");
            return Page();
        }

        var creada = solicitudes.Obtener(Solicitud.IdSolicitud) ?? Solicitud;
        bitacora.RegistrarCreacion(User, "solicitud", creada);
        TempData["Mensaje"] = "La solicitud se creo correctamente.";
        return RedirectToPage("Index");
    }

    private void CargarRepresentantes() => Representantes = representantes.Listar();
}

