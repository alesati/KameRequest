using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class DesgloseModel(SolicitudRepository solicitudes, DesgloseRepository desgloses, BitacoraRegistro bitacora) : PageModel
{
    public Solicitud? SolicitudActual { get; private set; }
    public List<DesgloseSolicitud> ListaDesgloses { get; private set; } = [];
    public decimal SumaMontos => ListaDesgloses.Sum(d => d.Monto);
    public decimal SumaIva => ListaDesgloses.Sum(d => d.Iva);
    public decimal TotalGeneral => ListaDesgloses.Sum(d => d.Total);

    [BindProperty]
    public DesgloseSolicitud NuevoDesglose { get; set; } = new();

    [BindProperty]
    public DesgloseSolicitud DesgloseEditar { get; set; } = new();

    [TempData]
    public string? MensajeError { get; set; }

    public IActionResult OnGet(int id) => CargarDatos(id) ? Page() : RedirectToPage("/Solicitudes/Index");
    public IActionResult OnPostAgregar(int idSolicitud)
    {
        if (!DatosValidos(NuevoDesglose))
        {
            MensajeError = "Revise el mes, año, monto y porcentaje de cobro.";
            return RedirectToPage(new { id = idSolicitud });
        }

        NuevoDesglose.IdSolicitud = idSolicitud;
        NuevoDesglose.IdDesglose = desgloses.Crear(NuevoDesglose);
        bitacora.RegistrarCreacion(User, "desglose", NuevoDesglose);
        return RedirectToPage(new { id = idSolicitud });
    }

    public IActionResult OnPostEditar(int idSolicitud)
    {
        if (!DatosValidos(DesgloseEditar))
        {
            MensajeError = "Revise el mes, año, monto y porcentaje de cobro.";
            return RedirectToPage(new { id = idSolicitud });
        }

        var anterior = desgloses.Obtener(DesgloseEditar.IdDesglose);
        if (anterior is null || anterior.IdSolicitud != idSolicitud)
            return RedirectToPage(new { id = idSolicitud });
        desgloses.Actualizar(DesgloseEditar);
        bitacora.RegistrarActualizacion(User, "desglose", anterior, desgloses.Obtener(DesgloseEditar.IdDesglose));
        return RedirectToPage(new { id = idSolicitud });
    }

    public IActionResult OnPostEliminar(int idDesglose, int idSolicitud)
    {
        var eliminado = desgloses.Obtener(idDesglose);
        if (eliminado is null || eliminado.IdSolicitud != idSolicitud)
            return RedirectToPage(new { id = idSolicitud });
        try
        {
            desgloses.Eliminar(idDesglose);
            bitacora.RegistrarEliminacion(User, "desglose", eliminado);
        }
        catch (MySqlException e) when (e.Number == 1451)
        {
            MensajeError = "No se puede eliminar un registro con datos relacionados.";
        }

        return RedirectToPage(new { id = idSolicitud });
    }

    private bool CargarDatos(int idSolicitud)
    {
        SolicitudActual = solicitudes.Obtener(idSolicitud);
        if (SolicitudActual is null)
            return false;
        ListaDesgloses = desgloses.ListarPorSolicitud(idSolicitud).ToList();
        return true;
    }

    private static bool DatosValidos(DesgloseSolicitud d) => d.Mes is >= 1 and <= 12 && d.Anio is >= 1000 and <= 9999 && d.Horas >= 0 && d.Monto >= 0 && d.PorcentajeCobro is >= 0 and <= 100;
}

