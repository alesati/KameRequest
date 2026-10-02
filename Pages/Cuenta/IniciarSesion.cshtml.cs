using System.Security.Claims;
using KameRequest.Models;
using KameRequest.Clases;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Cuenta;

public class IniciarSesionModel(UsuarioSeguridad seguridad) : PageModel
{
    // Recibe los datos que el usuario escribe en el formulario
    [BindProperty]
    public InicioSesion Datos { get; set; } = new();

    public IActionResult OnGet()
    {
        // Si ya inicio sesion no se le muestra otra vez el login
        return User.Identity?.IsAuthenticated == true ? RedirectToPage("/Index") : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Revisa los Required del modelo antes de consultar la base
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
                // Estos datos se guardan dentro de la cookie de sesion
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.NombreCompleto),
                new(ClaimTypes.GivenName, usuario.NombreCompleto),
                new(ClaimTypes.Role, usuario.TipoUsuario)
            };
            var identidad = new ClaimsIdentity(datosUsuario, CookieAuthenticationDefaults.AuthenticationScheme);
            // Crea la cookie que permite navegar por las paginas protegidas
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad));

            return RedirectToPage("/Index");
        }
        catch
        {
            // Muestra un mensaje controlado si falla la conexion u otro error tecnico
            ModelState.AddModelError(string.Empty, "No fue posible iniciar sesion, revise la conexion a la base de datos");
            return Page();
        }
    }

    private static string MensajeResultado(ResultadoAcceso resultado) => resultado switch
    {
        // Cambia el resultado interno por un mensaje que entiende el usuario
        ResultadoAcceso.UsuarioBloqueado => "El usuario se encuentra bloqueado",
        ResultadoAcceso.UsuarioInactivo => "El usuario se encuentra inactivo",
        _ => "Usuario o contraseña incorrectos"
    };
}


