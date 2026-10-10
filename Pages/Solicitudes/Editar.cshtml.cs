using KameRequest.Clases;
using KameRequest.Models;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class EditarModel(
    SolicitudRepository solicitudes,
    RepresentanteRepository representantes,
    BitacoraRegistro bitacora,
    NotificacionesCorreos notificaciones) : PageModel
{
    [BindProperty]
    public Solicitud Solicitud { get; set; } = new();
    public IReadOnlyList<Representante> Representantes { get; private set; } = [];
    public IReadOnlyList<EstadoSolicitud> Estados { get; private set; } = [];

    public IActionResult OnGet(int id)
    {
        Solicitud = solicitudes.Obtener(id) ?? new();
        CargarCatalogos();
        return Solicitud.IdSolicitud == 0 ? NotFound() : Page();
    }

    public IActionResult OnPost()
    {
        CargarCatalogos();
        if (!ModelState.IsValid)
            return Page();

        var anterior = solicitudes.Obtener(Solicitud.IdSolicitud);
        if (anterior is null)
            return NotFound();

        solicitudes.Actualizar(Solicitud);
        var actual = solicitudes.Obtener(Solicitud.IdSolicitud);
        bitacora.RegistrarActualizacion(User, "solicitud", anterior, actual);

        if (actual is not null && actual.IdEstado != anterior.IdEstado)
        {
            var representante = Representantes.FirstOrDefault(r => r.IdRepresentante == actual.IdRepresentante);
            var estadoAnterior = Estados.FirstOrDefault(e => e.IdEstado == anterior.IdEstado)?.Nombre ?? anterior.Estado ?? "Desconocido";
            var estadoNuevo = Estados.FirstOrDefault(e => e.IdEstado == actual.IdEstado)?.Nombre ?? actual.Estado ?? "Desconocido";
            if (!string.IsNullOrWhiteSpace(representante?.Email))
                notificaciones.EnviarCorreoCambioEstadoManual(User, representante.Email, estadoAnterior, estadoNuevo, actual.ConsecutivoOficio);
        }

        TempData["Mensaje"] = "La solicitud se actualizo correctamente.";
        return RedirectToPage("Index");
    }

    private void CargarCatalogos()
    {
        Representantes = representantes.Listar();
        Estados = solicitudes.ListarEstados();
    }
}
