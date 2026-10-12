using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages;

// pagina de bienvenida que se muestra despues de iniciar sesion
public class IndexModel : PageModel
{
    // carga los datos necesarios antes de mostrar la pantalla
    public IActionResult OnGet()
    {
        // si abre la raiz sin sesion se envia al login limpio
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToPage("/Cuenta/IniciarSesion");

        return Page();
    }
}
