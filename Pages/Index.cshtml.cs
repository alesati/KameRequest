using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages;

// Esta pagina solo se muestra despues de iniciar sesion
[Authorize]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
