using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Mantenimientos;

[Authorize(Roles = "Administrador")]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
