using KameRequest.Models;
using KameRequest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Usuarios;

[Authorize(Roles = "Administrador")]
public class CrearModel(UsuarioSeguridadService seguridad) : PageModel
{
    [BindProperty]
    public UsuarioFormulario Datos { get; set; } = new();

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();
        if (!seguridad.CrearUsuario(Datos, out var error))
        {
            ModelState.AddModelError(string.Empty, error!);
            return Page();
        }
        TempData["Mensaje"] = "Usuario creado correctamente";
        return RedirectToPage("Index");
    }
}
