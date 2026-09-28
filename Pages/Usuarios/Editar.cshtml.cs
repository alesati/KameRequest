using KameRequest.Models;
using KameRequest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Usuarios;

[Authorize(Roles = "Administrador")]
public class EditarModel(UsuarioSeguridadService seguridad, BitacoraService bitacora) : PageModel
{
    [BindProperty]
    public UsuarioFormulario Datos { get; set; } = new();

    public IActionResult OnGet(int id)
    {
        var usuario = seguridad.ObtenerUsuario(id);
        if (usuario is null) return NotFound();
        Datos = usuario;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        var anterior = seguridad.ObtenerUsuario(Datos.IdUsuario);
        if (anterior is null) return NotFound();

        if (!seguridad.ActualizarUsuario(Datos, out var error))
        {
            ModelState.AddModelError(string.Empty, error!);
            return Page();
        }

        bitacora.RegistrarActualizacion(User, "usuario",
            new
            {
                anterior.IdUsuario,
                anterior.NombreCompleto,
                anterior.NombreUsuario,
                anterior.Correo,
                anterior.TipoUsuario,
                anterior.Estado
            },
            new
            {
                Datos.IdUsuario,
                Datos.NombreCompleto,
                Datos.NombreUsuario,
                Datos.Correo,
                Datos.TipoUsuario,
                Datos.Estado
            });

        TempData["Mensaje"] = "Usuario actualizado correctamente";
        return RedirectToPage("Index");
    }
}
