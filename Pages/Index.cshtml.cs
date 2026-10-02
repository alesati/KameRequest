using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages;

// Esta pagina solo se muestra despues de iniciar sesion
[Authorize]
public class IndexModel : PageModel
{
    // El layout y la vista muestran los datos de la bienvenida
    public void OnGet()
    {
    }
}
