using KameRequest.Models;
using KameRequest.Clases;
using KameRequest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Usuarios;

[Authorize(Roles = "Administrador")]
public class EditarModel(UsuarioSeguridad seguridad, BitacoraService bitacora) : PageModel
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
        var anterior = seguridad.ObtenerUsuario(Datos.IdUsuario);
        if (!seguridad.ActualizarUsuario(Datos, out var error))
        {
            ModelState.AddModelError(string.Empty, error!);
            return Page();
        }
        TempData["Mensaje"] = "Usuario actualizado correctamente";
        bitacora.RegistrarActualizacion(User, "usuario", DatosBitacora(anterior), DatosBitacora(Datos));
        return RedirectToPage("Index");
    }

    private static object? DatosBitacora(UsuarioFormulario? usuario)
    {
        if (usuario is null) return null;

        return new
        {
            usuario.IdUsuario,
            usuario.NombreCompleto,
            usuario.NombreUsuario,
            usuario.Correo,
            usuario.TipoUsuario,
            usuario.Estado
        };
    }
}
