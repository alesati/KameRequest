using KameRequest.Models;
using KameRequest.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Usuarios;

[Authorize(Roles = "Administrador")]
public class EditarModel(UsuarioSeguridad seguridad) : PageModel
{
    // Contiene los datos del usuario que se va a modificar
    [BindProperty]
    public UsuarioFormulario Datos { get; set; } = new();

    public IActionResult OnGet(int id)
    {
        // Busca el registro segun el id recibido en la ruta
        var usuario = seguridad.ObtenerUsuario(id);
        if (usuario is null) return NotFound();
        Datos = usuario;
        return Page();
    }

    public IActionResult OnPost()
    {
        // Valida y luego guarda los cambios del formulario
        if (!ModelState.IsValid) return Page();
        if (!seguridad.ActualizarUsuario(Datos, out var error))
        {
            ModelState.AddModelError(string.Empty, error!);
            return Page();
        }
        TempData["Mensaje"] = "Usuario actualizado correctamente";
        return RedirectToPage("Index");
    }
}
