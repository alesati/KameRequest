// logica de la pantalla Editar.cshtml
using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Estados;

[Authorize]
public class EditarModel(EstadoRepository estados, BitacoraRegistro bitacora) : PageModel
{
    [BindProperty]
    public Estado Estado { get; set; } = new();

    public IActionResult OnGet(int id)
    {
        Estado = estados.Obtener(id) ?? new Estado();
        return Estado.idEstado == 0 ? NotFound() : Page();
    }

    public IActionResult OnPost()
    {
        var anterior = estados.Obtener(Estado.idEstado);
        if (anterior is null) return NotFound();
        estados.Actualizar(Estado);
        bitacora.RegistrarActualizacion(User, "estado", anterior, estados.Obtener(Estado.idEstado)!);
        TempData["Mensaje"] = "El estado se actualizo correctamente.";
        return RedirectToPage("Index");
    }
}

