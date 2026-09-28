using System.Security.Claims;
using KameRequest.Models;
using KameRequest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Usuarios;

[Authorize(Roles = "Administrador")]
public class IndexModel(UsuarioSeguridadService seguridad, BitacoraService bitacora) : PageModel
{
    private const int TamanoPagina = 10;
    public IEnumerable<Usuario> Usuarios { get; private set; } = [];
    public int PaginaActual { get; private set; }
    public int TotalPaginas { get; private set; }

    public void OnGet(int pagina = 1)
    {
        CargarUsuarios(pagina);
    }

    public IActionResult OnPostCambiarEstado(int idUsuario, string estado)
    {
        var anterior = seguridad.ObtenerUsuario(idUsuario);
        if (anterior is null)
        {
            TempData["Error"] = "No se encontro el usuario";
            return RedirectToPage();
        }

        if (!seguridad.CambiarEstadoUsuario(idUsuario, estado, out var error))
        {
            TempData["Error"] = error;
            return RedirectToPage();
        }

        bitacora.RegistrarActualizacion(User, "estado de usuario",
            new { anterior.IdUsuario, anterior.NombreUsuario, anterior.Estado },
            new { anterior.IdUsuario, anterior.NombreUsuario, Estado = estado });

        TempData["Mensaje"] = "El estado del usuario se actualizo";
        return RedirectToPage();
    }

    public IActionResult OnPostEliminar(int idUsuario)
    {
        var idUsuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idUsuarioActual == idUsuario.ToString())
        {
            TempData["Error"] = "No puede eliminar el usuario con el que inicio sesion";
            return RedirectToPage();
        }

        var eliminado = seguridad.ObtenerUsuario(idUsuario);
        if (eliminado is null)
        {
            TempData["Error"] = "No se encontro el usuario";
            return RedirectToPage();
        }

        if (!seguridad.EliminarUsuario(idUsuario, out var error))
        {
            TempData["Error"] = error;
            return RedirectToPage();
        }

        bitacora.RegistrarEliminacion(User, "usuario", new
        {
            eliminado.IdUsuario,
            eliminado.NombreCompleto,
            eliminado.NombreUsuario,
            eliminado.Correo,
            eliminado.TipoUsuario,
            eliminado.Estado
        });

        TempData["Mensaje"] = "El usuario se elimino correctamente";
        return RedirectToPage();
    }

    private void CargarUsuarios(int pagina)
    {
        var total = seguridad.ContarUsuarios();
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        PaginaActual = Math.Clamp(pagina, 1, TotalPaginas);
        Usuarios = seguridad.ListarUsuarios(PaginaActual, TamanoPagina);
    }
}

