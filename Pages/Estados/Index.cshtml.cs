// logica de la pantalla Index.cshtml
using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Estados;

[Authorize]
public class IndexModel(EstadoRepository estados, BitacoraRegistro bitacora) : PageModel
{
    private const int RegistrosPorPagina = 5;
    public IReadOnlyList<Estado> Estados { get; private set; } = [];
    public int PaginaActual { get; private set; }
    public int TotalPaginas { get; private set; }

    public void OnGet(int pagina = 1)
    {
        bitacora.RegistrarConsulta(User, "estados");
        var total = estados.Contar();
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)RegistrosPorPagina));
        PaginaActual = Math.Clamp(pagina, 1, TotalPaginas);
        Estados = estados.ObtenerPagina(PaginaActual, RegistrosPorPagina);
    }

    public IActionResult OnPostEliminar(int idEstado)
    {
        try
        {
            var eliminado = estados.Obtener(idEstado);
            if (eliminado is not null && estados.Eliminar(idEstado) == 1)
            {
                bitacora.RegistrarEliminacion(User, "estado", eliminado);
                TempData["Mensaje"] = "El estado se elimino correctamente.";
            }
        }
        catch (MySqlException excepcion) when (excepcion.Number == 1451)
        {
            TempData["Error"] = "No se puede eliminar un registro con datos relacionados.";
        }
        return RedirectToPage();
    }
}

