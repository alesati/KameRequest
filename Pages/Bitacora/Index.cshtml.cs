using KameRequest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Bitacora;

[Authorize]
public class IndexModel(BitacoraService bitacora) : PageModel
{
    private const int RegistrosPorPagina = 10;

    [BindProperty(SupportsGet = true)]
    public int Pagina { get; set; } = 1;

    public int TotalPaginas { get; private set; }
    public IReadOnlyList<KameRequest.Models.Bitacora> Registros { get; private set; } = [];

    public void OnGet()
    {
        bitacora.RegistrarConsulta(User, "bitacora general");
        var total = bitacora.ContarRegistros();
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)RegistrosPorPagina));
        Pagina = Math.Clamp(Pagina, 1, TotalPaginas);
        Registros = bitacora.ObtenerPagina(Pagina, RegistrosPorPagina);
    }
}
