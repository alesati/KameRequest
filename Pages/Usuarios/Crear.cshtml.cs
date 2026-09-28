using KameRequest.Models;
using KameRequest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Usuarios;

[Authorize(Roles = "Administrador")]
public class CrearModel(UsuarioSeguridadService seguridad, BitacoraService bitacora) : PageModel
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
        // La contrasena se excluye expresamente de la informacion enviada a la bitacora.
        bitacora.RegistrarCreacion(User, "usuario", new
        {
            Datos.NombreCompleto,
            Datos.NombreUsuario,
            Datos.Correo,
            Datos.TipoUsuario,
            Datos.Estado
        });
        TempData["Mensaje"] = "Usuario creado correctamente";
        return RedirectToPage("Index");
    }
}

