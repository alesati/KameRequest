using KameRequest.Services;
using KameRequest.Services.Abstract;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Cuenta;

[Authorize]
// cierra la sesion actual del usuario
public class CerrarSesionModel(IBitacoraService bitacora) : PageModel
{
    // El cierre se recibe por POST para no cerrar la sesion al abrir una direccion
    public async Task<IActionResult> OnPostAsync()
    {
        bitacora.Registrar(User, "Cierre de sesion.");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);// Borra la cookie actual antes de devolver al usuario al login
        return RedirectToPage("/Cuenta/IniciarSesion");
    }
}

