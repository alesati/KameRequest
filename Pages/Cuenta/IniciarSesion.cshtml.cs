using System.Security.Claims;
using KameRequest.Models;
using KameRequest.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Cuenta;

public class IniciarSesionModel(UsuarioSeguridadService seguridad) : PageModel
{
    [BindProperty]
    public InicioSesion Datos { get; set; } = new();

    public IActionResult OnGet()
    {
        return User.Identity?.IsAuthenticated == true ? RedirectToPage("/Index") : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var resultado = seguridad.ValidarAcceso(Datos, out var usuario);
            if (resultado != ResultadoAcceso.Correcto || usuario is null)
            {
                ModelState.AddModelError(string.Empty, MensajeResultado(resultado));
                return Page();
            }

            var datosUsuario = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.NombreCompleto),
                new(ClaimTypes.Role, usuario.TipoUsuario)
            };
            var identidad = new ClaimsIdentity(datosUsuario, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad));

            return RedirectToPage("/Index");
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "No fue posible iniciar sesion, revise la conexion a la base de datos");
            return Page();
        }
    }

    private static string MensajeResultado(ResultadoAcceso resultado) => resultado switch
    {
        ResultadoAcceso.UsuarioBloqueado => "El usuario se encuentra bloqueado",
        ResultadoAcceso.UsuarioInactivo => "El usuario se encuentra inactivo",
        _ => "Usuario o contraseña incorrectos"
    };
}
