using KameRequest.Services;
using KameRequest.Services.Abstract;
// logica de la pantalla Editar.cshtml
using KameRequest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Representantes;

[Authorize]
public class EditarModel(IRepresentanteService representantes, IBitacoraService bitacora) : PageModel
{
    [BindProperty]
    public Representante Representante { get; set; } = new();

    // carga los datos necesarios antes de mostrar la pantalla
    public IActionResult OnGet(int id)
    {
        Representante = representantes.Obtener(id) ?? new();
        return Representante.IdRepresentante == 0 ? NotFound() : Page();
    }

    // procesa los datos enviados desde el formulario
    public IActionResult OnPost()
    {
        var anterior = representantes.Obtener(Representante.IdRepresentante);
        if (anterior is null)
            return NotFound();
        representantes.Actualizar(Representante);
        bitacora.RegistrarActualizacion(User, "representante", anterior, representantes.Obtener(Representante.IdRepresentante));
        TempData["Mensaje"] = "El representante se actualizo correctamente.";
        return RedirectToPage("/Representantes/Index");
    }
}
