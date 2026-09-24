using KameRequest.Models;
using KameRequest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Usuarios;

[Authorize(Roles = "Administrador")]
public class IndexModel(UsuarioSeguridadService seguridad) : PageModel
{
    private const int TamanoPagina = 10;
    public IEnumerable<Usuario> Usuarios { get; private set; } = [];
    public int PaginaActual { get; private set; }
    public int TotalPaginas { get; private set; }

    public void OnGet(int pagina = 1)
    {
        var total = seguridad.ContarUsuarios();
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        PaginaActual = Math.Clamp(pagina, 1, TotalPaginas);
        Usuarios = seguridad.ListarUsuarios(PaginaActual, TamanoPagina);
    }
}
