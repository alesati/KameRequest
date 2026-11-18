using KameRequest.Services;
using KameRequest.Services.Abstract;
// logica de la pantalla Editar.cshtml
using KameRequest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Estados;

[Authorize]
public class EditarModel(IEstadoService estados, IBitacoraService bitacora) : PageModel
{
    [BindProperty]
    public Estado Estado { get; set; } = new();

    // carga los datos necesarios antes de mostrar la pantalla
    public IActionResult OnGet(int id)
    {
        Estado = estados.Obtener(id) ?? new Estado();
        return Estado.idEstado == 0 ? NotFound() : Page();
    }

    // procesa los datos enviados desde el formulario
    public IActionResult OnPost()
    {
        var anterior = estados.Obtener(Estado.idEstado);
        if (anterior is null) return NotFound();

        try
        {
            estados.Actualizar(Estado);
        }
        catch (ArgumentException error)
        {
            TempData["Error"] = error.Message;
            return RedirectToPage(new { id = Estado.idEstado });
        }

        bitacora.RegistrarActualizacion(User, "estado", anterior, estados.Obtener(Estado.idEstado)!);
        TempData["Mensaje"] = "El estado se actualizo correctamente.";
        return RedirectToPage("/Estados/Index");
    }
}
