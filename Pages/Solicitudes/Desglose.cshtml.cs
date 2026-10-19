using KameRequest.Services;
using KameRequest.Services.Abstract;
// logica de la pantalla Desglose.cshtml
using KameRequest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class DesgloseModel(ISolicitudService solicitudes, IDesgloseService desgloses, IBitacoraService bitacora) : PageModel
{
    private const int RegistrosPorPagina = 5;
    public Solicitud? SolicitudActual { get; private set; }
    public List<DesgloseSolicitud> ListaDesgloses { get; private set; } = [];
    public int PaginaActual { get; private set; }
    public int TotalPaginas { get; private set; }
    public decimal SumaMontos => ListaDesgloses.Sum(d => d.Monto);
    public decimal SumaIva => ListaDesgloses.Sum(d => d.Iva);
    public decimal TotalGeneral => ListaDesgloses.Sum(d => d.Total);

    [BindProperty]
    public DesgloseSolicitud NuevoDesglose { get; set; } = new();

    [BindProperty]
    public DesgloseSolicitud DesgloseEditar { get; set; } = new();

    [TempData]
    public string? MensajeError { get; set; }

    // carga los datos necesarios antes de mostrar la pantalla
    public IActionResult OnGet(int id, int pagina = 1) => CargarDatos(id, pagina) ? Page() : RedirectToPage("/Solicitudes/Index");
    // valida y agrega el nuevo dato relacionado
    public IActionResult OnPostAgregar(int idSolicitud, bool modal = false)
    {
        if (!DatosValidos(NuevoDesglose))
        {
            MensajeError = "Revise el mes, año, monto y porcentaje de cobro.";
            return VolverAListado(idSolicitud, modal);
        }

        if (desgloses.SumarPorcentajePorSolicitud(idSolicitud) + NuevoDesglose.PorcentajeCobro > 100)


        {


            MensajeError = "El porcentaje total de cobro no puede ser mayor a 100%.";


            return VolverAListado(idSolicitud, modal);


        }



        NuevoDesglose.IdSolicitud = idSolicitud;


        NuevoDesglose.IdDesglose = desgloses.Crear(NuevoDesglose);
        bitacora.RegistrarCreacion(User, "desglose", NuevoDesglose);
        return VolverAListado(idSolicitud, modal);
    }

    // valida y guarda los cambios del formulario
    public IActionResult OnPostEditar(int idSolicitud, bool modal = false)
    {
        if (!DatosValidos(DesgloseEditar))
        {
            MensajeError = "Revise el mes, año, monto y porcentaje de cobro.";
            return VolverAListado(idSolicitud, modal);
        }

        var anterior = desgloses.Obtener(DesgloseEditar.IdDesglose);
        if (anterior is null || anterior.IdSolicitud != idSolicitud)

            return VolverAListado(idSolicitud, modal);


        if (desgloses.SumarPorcentajePorSolicitud(idSolicitud, DesgloseEditar.IdDesglose) + DesgloseEditar.PorcentajeCobro > 100)

        {

            MensajeError = "El porcentaje total de cobro no puede ser mayor a 100%.";

            return VolverAListado(idSolicitud, modal);

        }


        desgloses.Actualizar(DesgloseEditar);
        bitacora.RegistrarActualizacion(User, "desglose", anterior, desgloses.Obtener(DesgloseEditar.IdDesglose));
        return VolverAListado(idSolicitud, modal);
    }

    // confirma y procesa la eliminacion del registro
    public IActionResult OnPostEliminar(int idDesglose, int idSolicitud, bool modal = false)
    {
        var eliminado = desgloses.Obtener(idDesglose);
        if (eliminado is null || eliminado.IdSolicitud != idSolicitud)
            return VolverAListado(idSolicitud, modal);
        try
        {
            desgloses.Eliminar(idDesglose);
            bitacora.RegistrarEliminacion(User, "desglose", eliminado);
        }
        catch (MySqlException e) when (e.Number == 1451)
        {
            MensajeError = "No se puede eliminar un registro con datos relacionados.";
        }

        return VolverAListado(idSolicitud, modal);
    }

    // conserva la vista reducida cuando la accion viene desde el modal de solicitudes
    private RedirectToPageResult VolverAListado(int idSolicitud, bool modal) =>
        RedirectToPage(new { id = idSolicitud, modal = modal ? 1 : (int?)null });

    private bool CargarDatos(int idSolicitud, int pagina)
    {
        SolicitudActual = solicitudes.Obtener(idSolicitud);
        if (SolicitudActual is null)
            return false;

        var total = desgloses.ContarPorSolicitud(idSolicitud);
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)RegistrosPorPagina));
        PaginaActual = Math.Clamp(pagina, 1, TotalPaginas);
        ListaDesgloses = desgloses.ObtenerPaginaPorSolicitud(idSolicitud, PaginaActual, RegistrosPorPagina).ToList();
        return true;
    }

    private static bool DatosValidos(DesgloseSolicitud d)
    {
        return d.Mes is >= 1 and <= 12
            && d.Anio is >= 1000 and <= 9999
            && d.Horas >= 0
            && d.Monto >= 0
            && d.PorcentajeCobro is >= 0 and <= 100;
    }
}
