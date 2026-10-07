using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class IndexModel(SolicitudRepository solicitudes, BitacoraRegistro bitacora) : PageModel
{
    private const int RegistrosPorPagina = 5;
    public IReadOnlyList<Solicitud> Solicitudes { get; private set; } = [];
    public int PaginaActual { get; private set; }
    public int TotalPaginas { get; private set; }

    public void OnGet(int pagina = 1)
    {
        bitacora.RegistrarConsulta(User, "solicitudes");
        var total = solicitudes.Contar();
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)RegistrosPorPagina));
        PaginaActual = Math.Clamp(pagina, 1, TotalPaginas);
        Solicitudes = solicitudes.ObtenerPagina(PaginaActual, RegistrosPorPagina);
    }

    public IActionResult OnPostEliminar(int idSolicitud)
    {
        try
        {
            var eliminada = solicitudes.Obtener(idSolicitud);
            if (eliminada is not null && solicitudes.Eliminar(idSolicitud) == 1)
            {
                bitacora.RegistrarEliminacion(User, "solicitud", eliminada);
                TempData["Mensaje"] = "La solicitud se elimino correctamente.";
            }
        }
        catch (SqlException e) when (e.Number == 547)
        {
            TempData["Error"] = "No se puede eliminar un registro con datos relacionados.";
        }

        return RedirectToPage();
    }

    public static DateTime AgregarDiasHabiles(DateTime fechaIngreso, int cantidadDias)
    {
        var fecha = fechaIngreso.Date;
        var agregados = 0;
        while (agregados < cantidadDias)
        {
            fecha = fecha.AddDays(1);
            if (fecha.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                agregados++;
        }

        return fecha;
    }
}

