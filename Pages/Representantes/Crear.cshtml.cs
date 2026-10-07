using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Representantes;

[Authorize]
public class CrearModel(RepresentanteRepository representantes, BitacoraRegistro bitacora) : PageModel
{
    [BindProperty]
    public Representante Representante { get; set; } = new();

    public IActionResult OnPost()
    {
        Representante.IdRepresentante = representantes.Crear(Representante);
        bitacora.RegistrarCreacion(User, "representante", Representante);
        TempData["Mensaje"] = "El representante se creo correctamente.";
        return RedirectToPage("Index");
    }
}
