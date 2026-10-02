using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages;

// Modelo basico de la pagina de privacidad creada por la plantilla Razor
public class PrivacyModel : PageModel
{
    // Logger disponible si se necesita registrar un error de esta pagina
    private readonly ILogger<PrivacyModel> _logger;

    public PrivacyModel(ILogger<PrivacyModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        // La vista es estatica y no necesita cargar datos
    }
}

