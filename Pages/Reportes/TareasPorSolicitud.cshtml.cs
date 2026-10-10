using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KameRequest.Pages.Reportes;

[Authorize]
public class TareasPorSolicitudModel(SolicitudRepository solicitudes, ReporteRepository reportes, BitacoraRegistro bitacora) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int IdSolicitudSeleccionada { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Mes { get; set; } = DateTime.Now.Month;

    [BindProperty(SupportsGet = true)]
    public int Anio { get; set; } = DateTime.Now.Year;
    public List<SelectListItem> ListaSolicitudes { get; set; } = [];
    public List<TareaReporteDto> ListaReporte { get; set; } = [];
    public decimal TotalHoras => ListaReporte.Sum(t => t.HorasInvertidas);
    public bool BusquedaRealizada { get; set; }
    public string InfoSolicitud { get; set; } = string.Empty;

    public void OnGet()
    {
        ListaSolicitudes = solicitudes.ListarParaReporte().Select(s => new SelectListItem { Value = s.IdSolicitud.ToString(), Text = $"{s.ConsecutivoOficio} - {s.Titulo}", Selected = s.IdSolicitud == IdSolicitudSeleccionada }).ToList();
        if (IdSolicitudSeleccionada > 0 && Mes > 0 && Anio > 0)
        {
            BusquedaRealizada = true;
            ListaReporte = reportes.TareasPorSolicitud(IdSolicitudSeleccionada, Mes, Anio).ToList();
            InfoSolicitud = ListaReporte.Any() ? $"{ListaReporte[0].ConsecutivoOficio} - {ListaReporte[0].TituloSolicitud}" : ListaSolicitudes.FirstOrDefault(x => x.Value == IdSolicitudSeleccionada.ToString())?.Text ?? "Solicitud Seleccionada";
            bitacora.RegistrarConsulta(User, "reporte de tareas por solicitud");
        }
    }
}

