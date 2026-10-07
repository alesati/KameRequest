using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Representantes;

[Authorize]
public class EditarModel(RepresentanteRepository representantes, BitacoraRegistro bitacora) : PageModel
{
    [BindProperty]
    public Representante Representante { get; set; } = new();

    public IActionResult OnGet(int id)
    {
        Representante = representantes.Obtener(id) ?? new();
        return Representante.IdRepresentante == 0 ? NotFound() : Page();
    }

    public IActionResult OnPost()
    {
        var anterior = representantes.Obtener(Representante.IdRepresentante);
        if (anterior is null)
            return NotFound();
        representantes.Actualizar(Representante);
        bitacora.RegistrarActualizacion(User, "representante", anterior, representantes.Obtener(Representante.IdRepresentante));
        TempData["Mensaje"] = "El representante se actualizo correctamente.";
        return RedirectToPage("Index");
    }
}
