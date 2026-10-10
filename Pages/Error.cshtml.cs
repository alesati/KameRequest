using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
// Pagina que se usa cuando ocurre un error no controlado en la aplicacion
// controla la pantalla que aparece ante un error no esperado
public class ErrorModel : PageModel
{
    // Identificador util para revisar un error especifico en los registros
    public string? RequestId { get; set; }
    // Solo muestra el identificador si se logro obtener uno
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    private readonly ILogger<ErrorModel> _logger;
    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        // Toma el id de la solicitud actual para mostrarlo en la vista de error
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }
}

