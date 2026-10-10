// logica de la pantalla Index.cshtml
using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Representantes;

[Authorize]
public class IndexModel(RepresentanteRepository representantes, BitacoraRegistro bitacora) : PageModel
{
    private const int RegistrosPorPagina = 10;
    public IReadOnlyList<Representante> Representantes { get; private set; } = [];
    public int PaginaActual { get; private set; }
    public int TotalPaginas { get; private set; }

    public void OnGet(int pagina = 1)
    {
        bitacora.RegistrarConsulta(User, "representantes");
        var total = representantes.Contar();
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)RegistrosPorPagina));
        PaginaActual = Math.Clamp(pagina, 1, TotalPaginas);
        Representantes = representantes.ObtenerPagina(PaginaActual, RegistrosPorPagina);
    }

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


