using KameRequest.Models;
using KameRequest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Usuarios;

[Authorize(Roles = "Administrador")]
public class IndexModel(UsuarioSeguridadService seguridad) : PageModel
{
    public IEnumerable<Usuario> Usuarios { get; private set; } = [];

    public void OnGet()
    {
        Usuarios = seguridad.ListarUsuarios();
    }
}
