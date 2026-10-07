using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KameRequest.Pages.Reportes;

public class TareasPorUsuarioModel(ReporteRepository reportes, BitacoraRegistro bitacora) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? UsuarioSeleccionado { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Mes { get; set; } = DateTime.Now.Month;

    [BindProperty(SupportsGet = true)]
    public int Anio { get; set; } = DateTime.Now.Year;
    public List<SelectListItem> ListaUsuarios { get; set; } = [];
    public List<TareaReporteDto> ListaReporte { get; set; } = [];
    public decimal TotalHoras => ListaReporte.Sum(t => t.HorasInvertidas);
    public bool BusquedaRealizada { get; set; }

    public void OnGet()
    {
        ListaUsuarios = reportes.ListarUsuariosConTareas().Select(u => new SelectListItem { Value = u, Text = u, Selected = u == UsuarioSeleccionado }).ToList();
        if (!string.IsNullOrWhiteSpace(UsuarioSeleccionado) && Mes > 0 && Anio > 0)
        {
            BusquedaRealizada = true;
            ListaReporte = reportes.TareasPorUsuario(UsuarioSeleccionado, Mes, Anio).ToList();
            bitacora.RegistrarConsulta(User, "reporte de tareas por usuario");
        }
    }
}
