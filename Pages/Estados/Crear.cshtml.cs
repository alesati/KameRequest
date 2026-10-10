// logica de la pantalla Crear.cshtml
using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Estados;

[Authorize]
public class CrearModel(EstadoRepository estados, BitacoraRegistro bitacora) : PageModel
{
    [BindProperty]
    public Estado Estado { get; set; } = new();

    public IActionResult OnPost()
    {
        Estado.idEstado = estados.Crear(Estado);
        bitacora.RegistrarCreacion(User, "estado", estados.Obtener(Estado.idEstado)!);
        TempData["Mensaje"] = "El estado se creo correctamente.";
        return RedirectToPage("Index");
    }
}

