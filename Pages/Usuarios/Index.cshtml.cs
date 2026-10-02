using System.Security.Claims;
using KameRequest.Models;
using KameRequest.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Usuarios;

[Authorize(Roles = "Administrador")]
public class IndexModel(UsuarioSeguridad seguridad) : PageModel
{
    // Cantidad de registros que se muestran por pagina
    private const int TamanoPagina = 10;
    // Lista y datos necesarios para dibujar la tabla paginada
    public IEnumerable<Usuario> Usuarios { get; private set; } = [];
    public int PaginaActual { get; private set; }
    public int TotalPaginas { get; private set; }

    public void OnGet(int pagina = 1)
    {
        // Carga la pagina solicitada al entrar al mantenimiento
        CargarUsuarios(pagina);
    }

    public IActionResult OnPostCambiarEstado(int idUsuario, string estado)
    {
        // Recibe el estado elegido en la lista y lo actualiza en la base
        if (!seguridad.CambiarEstadoUsuario(idUsuario, estado, out var error))
        {
            TempData["Error"] = error;
            return RedirectToPage();
        }

        TempData["Mensaje"] = "El estado del usuario se actualizo";
        return RedirectToPage();
    }

    public IActionResult OnPostEliminar(int idUsuario)
    {
        // Evita que el administrador borre su propia cuenta mientras esta usandola
        var idUsuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idUsuarioActual == idUsuario.ToString())
        {
            TempData["Error"] = "No puede eliminar el usuario con el que inicio sesion";
            return RedirectToPage();
        }

        if (!seguridad.EliminarUsuario(idUsuario, out var error))
        {
            TempData["Error"] = error;
            return RedirectToPage();
        }

        TempData["Mensaje"] = "El usuario se elimino correctamente";
        return RedirectToPage();
    }

    private void CargarUsuarios(int pagina)
    {
        // Calcula las paginas usando la cantidad total de usuarios
        var total = seguridad.ContarUsuarios();
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        PaginaActual = Math.Clamp(pagina, 1, TotalPaginas);
        // Pide solamente los usuarios que se necesitan para esta pagina
        Usuarios = seguridad.ListarUsuarios(PaginaActual, TamanoPagina);
    }
}
