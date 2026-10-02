using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Mantenimientos;

[Authorize(Roles = "Administrador")]
// Pagina principal de los catalogos que solo puede abrir un administrador
public class IndexModel : PageModel
{
    // Por ahora la vista no necesita consultar datos antes de mostrarse
    public void OnGet()
    {
    }
}
