using KameRequest.Services;
using KameRequest.Services.Abstract;
// logica de la pantalla FacturacionMes.cshtml
using KameRequest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Reportes;

[Authorize]
public class FacturacionMesModel(IReporteService reportes, IBitacoraService bitacora) : PageModel
{
    private const int RegistrosPorPagina = 10;

    [BindProperty(SupportsGet = true)]
    public int Mes { get; set; } = DateTime.Now.Month;
    [BindProperty(SupportsGet = true)]
    public int Anio { get; set; } = DateTime.Now.Year;
    [BindProperty(SupportsGet = true)]
    public int PaginaActual { get; set; } = 1;
    public List<Rpt_FacturacionMes> ListaReporte { get; set; } = [];
    public int TotalPaginas { get; set; }
    public decimal Subtotal { get; private set; }
    public decimal TotalIva { get; private set; }
    public decimal Total { get; private set; }
    public bool BusquedaRealizada { get; set; }

    // carga los datos necesarios antes de mostrar la pantalla
    public void OnGet()
    {
        if (Mes is < 1 or > 12 || Anio < 1000) return;
        BusquedaRealizada = true;
        var totalRegistros = reportes.ContarFacturacionPorMes(Mes, Anio);
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)RegistrosPorPagina));
        PaginaActual = Math.Clamp(PaginaActual, 1, TotalPaginas);
        ListaReporte = reportes.FacturacionPorMes(Mes, Anio, PaginaActual, RegistrosPorPagina).ToList();

        var totales = reportes.TotalesFacturacionPorMes(Mes, Anio);
        Subtotal = totales.Monto;
        TotalIva = totales.Iva;
        Total = totales.Total;
        bitacora.RegistrarConsulta(User, "reporte de facturacion mensual");
    }
}

