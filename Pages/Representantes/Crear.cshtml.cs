using KameRequest.Services;
using KameRequest.Services.Abstract;
// logica de la pantalla Crear.cshtml
using KameRequest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Representantes;

[Authorize]
public class CrearModel(IRepresentanteService representantes, IBitacoraService bitacora) : PageModel
{
    [BindProperty]
    public Representante Representante { get; set; } = new();

    // procesa los datos enviados desde el formulario
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();
        Representante.IdRepresentante = representantes.Crear(Representante);
        bitacora.RegistrarCreacion(User, "representante", Representante);
        TempData["Mensaje"] = "El representante se creo correctamente.";
        return RedirectToPage("Index");
    }
}


