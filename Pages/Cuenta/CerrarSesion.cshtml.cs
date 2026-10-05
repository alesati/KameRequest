using KameRequest.Clases;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Cuenta;

[Authorize]
public class CerrarSesionModel(BitacoraRegistro bitacora) : PageModel
{
    // El cierre se recibe por POST para no cerrar la sesion al abrir una direccion
    public async Task<IActionResult> OnPostAsync()
    {
        bitacora.Registrar(User, "Cierre de sesion.");
        // Borra la cookie actual antes de devolver al usuario al login
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Cuenta/IniciarSesion");
    }
}
