using KameRequest.Models;
using KameRequest.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace KameRequest.Pages.Bitacora;

// Consulta las acciones registradas en el sistema.
public class IndexModel(BitacoraService bitacora) : PageModel
{
    private const int RegistrosPorPagina = 10;

    [BindProperty(SupportsGet = true)]
    public int Pagina { get; set; } = 1;

    public int TotalPaginas { get; private set; }
    public IReadOnlyList<KameRequest.Models.Bitacora> Registros { get; private set; } = [];

    // Calcula la pagina solicitada y carga diez registros de bitacora.
    public void OnGet()
    {
        bitacora.Registrar(User, "El usuario consulta bitácora general.");
        var total = bitacora.ContarRegistros();
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)RegistrosPorPagina));
        Pagina = Math.Clamp(Pagina, 1, TotalPaginas);
        Registros = bitacora.ObtenerPagina(Pagina, RegistrosPorPagina);
    }
}


