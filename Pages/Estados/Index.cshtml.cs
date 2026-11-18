using KameRequest.Services;
using KameRequest.Services.Abstract;
// logica de la pantalla Index.cshtml
using KameRequest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Estados;

[Authorize]
public class IndexModel(IEstadoService estados, IBitacoraService bitacora) : PageModel
{
    private const int RegistrosPorPagina = 5;
    public IReadOnlyList<Estado> Estados { get; private set; } = [];
    public int PaginaActual { get; private set; }
    public int TotalPaginas { get; private set; }

    // carga los datos necesarios antes de mostrar la pantalla
    public void OnGet(int pagina = 1)
    {
        bitacora.RegistrarConsulta(User, "estados");
        var total = estados.Contar();
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)RegistrosPorPagina));
        PaginaActual = Math.Clamp(pagina, 1, TotalPaginas);
        Estados = estados.ObtenerPagina(PaginaActual, RegistrosPorPagina);
    }

    // confirma y procesa la eliminacion del registro
    public IActionResult OnPostEliminar(int idEstado)
    {
        try
        {
            var eliminado = estados.EliminarSeguro(idEstado);
            if (eliminado is not null)
            {
                bitacora.RegistrarEliminacion(User, "estado", eliminado);
                TempData["Mensaje"] = "El estado se elimino correctamente.";
            }
        }
        catch (InvalidOperationException error)
        {
            TempData["Error"] = error.Message;
        }
        return RedirectToPage();
    }
}

