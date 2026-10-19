using KameRequest.Services;
using KameRequest.Services.Abstract;
// logica de la pantalla Index.cshtml
using KameRequest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Representantes;

[Authorize]
public class IndexModel(IRepresentanteService representantes, IBitacoraService bitacora) : PageModel
{
    private const int RegistrosPorPagina = 10;
    public IReadOnlyList<Representante> Representantes { get; private set; } = [];
    public int PaginaActual { get; private set; }
    public int TotalPaginas { get; private set; }

    // carga los datos necesarios antes de mostrar la pantalla
    public void OnGet(int pagina = 1)
    {
        bitacora.RegistrarConsulta(User, "representantes");
        var total = representantes.Contar();
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)RegistrosPorPagina));
        PaginaActual = Math.Clamp(pagina, 1, TotalPaginas);
        Representantes = representantes.ObtenerPagina(PaginaActual, RegistrosPorPagina);
    }

    // confirma y procesa la eliminacion del registro
    public IActionResult OnPostEliminar(int idRepresentante)
    {
        try
        {
            var eliminado = representantes.Obtener(idRepresentante);
            if (eliminado is not null && representantes.Eliminar(idRepresentante) == 1)
            {
                bitacora.RegistrarEliminacion(User, "representante", eliminado);
                TempData["Mensaje"] = "El representante se elimino correctamente.";
            }
        }
        catch (MySqlException e) when (e.Number == 1451)
        {
            TempData["Error"] = "No se puede eliminar un registro con datos relacionados.";
        }

        return RedirectToPage();
    }
}


