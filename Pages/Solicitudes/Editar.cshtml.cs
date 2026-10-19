using KameRequest.Services;
using KameRequest.Services.Abstract;
// logica de la pantalla Editar.cshtml
using KameRequest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class EditarModel(
    ISolicitudService solicitudes,
    IRepresentanteService representantes,
    IBitacoraService bitacora,
    INotificacionesCorreoService notificaciones) : PageModel
{
    [BindProperty]
    public Solicitud Solicitud { get; set; } = new();
    public IReadOnlyList<Representante> Representantes { get; private set; } = [];
    public IReadOnlyList<EstadoSolicitud> Estados { get; private set; } = [];

    // carga los datos necesarios antes de mostrar la pantalla
    public IActionResult OnGet(int id)
    {
        Solicitud = solicitudes.Obtener(id) ?? new();
        CargarCatalogos();
        return Solicitud.IdSolicitud == 0 ? NotFound() : Page();
    }

    // procesa los datos enviados desde el formulario
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
        return RedirectToPage("/Solicitudes/Index");
    }

    private void CargarCatalogos()
    {
        Representantes = representantes.Listar();
        Estados = solicitudes.ListarEstados();
    }
}
