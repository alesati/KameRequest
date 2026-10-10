// logica de la pantalla FacturacionMes.cshtml
using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Reportes;

[Authorize]
public class FacturacionMesModel(ReporteRepository reportes, BitacoraRegistro bitacora) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int Mes { get; set; } = DateTime.Now.Month;
    [BindProperty(SupportsGet = true)]
    public int Anio { get; set; } = DateTime.Now.Year;
    public List<Rpt_FacturacionMes> ListaReporte { get; set; } = [];
    public decimal Subtotal => ListaReporte.Sum(t => t.Monto);
    public decimal TotalIva => ListaReporte.Sum(t => t.Iva);
    public decimal Total => ListaReporte.Sum(t => t.Total);
    public bool BusquedaRealizada { get; set; }

    public void OnGet()
    {
        if (Mes is < 1 or > 12 || Anio < 1000) return;
        BusquedaRealizada = true;
        ListaReporte = reportes.FacturacionPorMes(Mes, Anio).ToList();
        bitacora.RegistrarConsulta(User, "reporte de facturacion mensual");
    }
}

