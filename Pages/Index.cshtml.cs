using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages;

// pagina de bienvenida que se muestra despues de iniciar sesion
[Authorize]
public class IndexModel : PageModel
{
    // carga los datos necesarios antes de mostrar la pantalla
    public void OnGet()
    {
        // la autorizacion se valida antes de llegar a esta pagina
    }
}
