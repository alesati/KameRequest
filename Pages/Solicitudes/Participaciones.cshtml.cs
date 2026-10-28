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

    public IActionResult OnGet(int id)
    {
        Solicitud = solicitudes.Obtener(id);
        if (Solicitud is null)
            return NotFound();

        Lista = participaciones.ListarPorSolicitud(id);
        return Page();
    }

    public IActionResult OnPostAprobar(int idSolicitud, int idParticipacion)
    {
        var anterior = participaciones.ListarPorSolicitud(idSolicitud)
            .SingleOrDefault(item => item.IdParticipacion == idParticipacion);

        if (anterior is null)
            return NotFound();

        if (!participaciones.Aprobar(idSolicitud, idParticipacion))
        {
            TempData["Error"] = "Ya existe una propuesta aprobada o la participación no corresponde a la solicitud.";
            return RedirectToPage(new { id = idSolicitud });
        }

        bitacora.RegistrarActualizacion(User, "participacion de solicitud", anterior, new { anterior.IdParticipacion, Estado = "Aprobada" });
        TempData["Mensaje"] = "La propuesta se aprobó correctamente.";
        return RedirectToPage(new { id = idSolicitud });
    }
}