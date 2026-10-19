using KameRequest.Services;
using KameRequest.Services.Abstract;
// logica de la pantalla TareasPorUsuario.cshtml
using KameRequest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KameRequest.Pages.Reportes;

[Authorize]
public class TareasPorUsuarioModel(IReporteService reportes, IBitacoraService bitacora) : PageModel
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

    // carga los datos necesarios antes de mostrar la pantalla
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


