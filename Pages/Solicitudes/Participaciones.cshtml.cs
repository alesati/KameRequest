using KameRequest.Models;
using KameRequest.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class ParticipacionesModel(
    ISolicitudService solicitudes,
    IParticipacionService participaciones,
    IBitacoraService bitacora) : PageModel
{
    public Solicitud? Solicitud { get; private set; }
    public IReadOnlyList<ParticipacionSolicitud> Lista { get; private set; } = [];
    public ParticipacionSolicitud? ParticipacionSeleccionada { get; private set; }
    public bool TienePropuestaAprobada => Lista.Any(item => item.Estado == "Aprobada");

    // carga el listado o el detalle de una propuesta recibida
    public IActionResult OnGet(int id, int? participacion)
    {
        Solicitud = solicitudes.Obtener(id);
        if (Solicitud is null)
            return NotFound();

        Lista = participaciones.ListarPorSolicitud(id);
        bitacora.RegistrarConsulta(User, "participaciones de solicitud");

        if (participacion.HasValue)
        {
            ParticipacionSeleccionada = Lista.SingleOrDefault(item => item.IdParticipacion == participacion.Value);
            if (ParticipacionSeleccionada is null)
                return NotFound();
        }

        return Page();
    }

    // aprueba una propuesta y la convierte en el desglose oficial
    public IActionResult OnPostAprobar(int idSolicitud, int idParticipacion, bool modal = false)
    {
        var anterior = participaciones.ListarPorSolicitud(idSolicitud)
            .SingleOrDefault(item => item.IdParticipacion == idParticipacion);

        if (anterior is null)
            return NotFound();

        try
        {
            if (!participaciones.Aprobar(idSolicitud, idParticipacion))
            {
                TempData["Error"] = "Ya existe una propuesta aprobada o la participación no corresponde a la solicitud.";
                return RedirigirDetalle(idSolicitud, idParticipacion, modal);
            }

            bitacora.RegistrarActualizacion(User, "participacion de solicitud", anterior, new { anterior.IdParticipacion, Estado = "Aprobada" });
            TempData["Mensaje"] = "La propuesta se aprobó y ahora es el desglose oficial de la solicitud.";
        }
        catch (InvalidOperationException error)
        {
            TempData["Error"] = error.Message;
        }

        return RedirigirDetalle(idSolicitud, idParticipacion, modal);
    }

    // conserva el modal abierto cuando la aprobacion se hizo desde solicitudes
    private RedirectToPageResult RedirigirDetalle(int idSolicitud, int idParticipacion, bool modal) =>
        RedirectToPage(new { id = idSolicitud, participacion = idParticipacion, modal = modal ? 1 : (int?)null });
}
