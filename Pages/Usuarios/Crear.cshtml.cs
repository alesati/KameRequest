using KameRequest.Models;
using KameRequest.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Usuarios;

[Authorize(Roles = "Administrador")]
public class CrearModel(UsuarioSeguridad seguridad, BitacoraRegistro bitacora) : PageModel
{
    // Se llena con los valores escritos en el formulario
    [BindProperty]
    public UsuarioFormulario Datos { get; set; } = new();

    public void OnGet() { }

    public IActionResult OnPost()
    {
        // Si falta algun dato requerido vuelve a mostrar la misma pagina
        if (!ModelState.IsValid) return Page();
        if (!seguridad.CrearUsuario(Datos, out var error))
        {
            ModelState.AddModelError(string.Empty, error!);
            return Page();
        }
        TempData["Mensaje"] = "Usuario creado correctamente";
        bitacora.RegistrarCreacion(User, "usuario", DatosBitacora(Datos));
        // TempData permite mostrar el mensaje despues de redirigir al listado
        return RedirectToPage("Index");
    }

    private static object DatosBitacora(UsuarioFormulario usuario) => new
    {
        usuario.IdUsuario,
        usuario.NombreCompleto,
        usuario.NombreUsuario,
        usuario.Correo,
        usuario.TipoUsuario,
        usuario.Estado
    };
}
