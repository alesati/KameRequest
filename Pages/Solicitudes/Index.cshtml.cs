using KameRequest.Services;
using KameRequest.Services.Abstract;
// logica de la pantalla Index.cshtml
using KameRequest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class IndexModel(ISolicitudService solicitudes, IRepresentanteService representantes, IBitacoraService bitacora) : PageModel
{
    private const int RegistrosPorPagina = 5;
    public IReadOnlyList<Solicitud> Solicitudes { get; private set; } = [];
    public int PaginaActual { get; private set; }
    public int TotalPaginas { get; private set; }
    public IReadOnlyList<Representante> Representantes { get; private set; } = [];
    public IReadOnlyList<EstadoSolicitud> Estados { get; private set; } = [];
    public int? PublicacionConError { get; private set; }
    public string? ErrorPublicacion { get; private set; }

    [BindProperty]
    public int IdSolicitudPublicacion { get; set; }

    [BindProperty]
    public bool PublicadaExterna { get; set; }

    [BindProperty]
    public DateTime? FechaVencimientoPublicacion { get; set; }

    // carga los datos necesarios antes de mostrar la pantalla
    public void OnGet(int pagina = 1, int? publicacion = null)
    {
        PublicacionConError = publicacion;
        ErrorPublicacion = TempData["ErrorPublicacion"] as string;
        bitacora.RegistrarConsulta(User, "solicitudes");
        var total = solicitudes.Contar();
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)RegistrosPorPagina));
        PaginaActual = Math.Clamp(pagina, 1, TotalPaginas);
        Solicitudes = solicitudes.ObtenerPagina(PaginaActual, RegistrosPorPagina);
        Representantes = representantes.Listar();
        Estados = solicitudes.ListarEstados();
    }

    // confirma y procesa la eliminacion del registro
    public IActionResult OnPostEliminar(int idSolicitud)
    {
        try
        {
            var eliminada = solicitudes.EliminarSeguro(idSolicitud);
            if (eliminada is not null)
            {
                bitacora.RegistrarEliminacion(User, "solicitud", eliminada);
                TempData["Mensaje"] = "La solicitud se elimino correctamente.";
            }
        }
        catch (InvalidOperationException error)
        {
            TempData["Error"] = error.Message;
        }

        return RedirectToPage();
    }

    // Guarda si la solicitud aparece en el modulo externo y hasta que fecha.
    public IActionResult OnPostGuardarPublicacion()
    {
        var anterior = solicitudes.Obtener(IdSolicitudPublicacion);
        if (anterior is null)
            return NotFound();

        try
        {
            solicitudes.ActualizarPublicacionExterna(
                IdSolicitudPublicacion,
                PublicadaExterna,
                FechaVencimientoPublicacion);

            var actual = solicitudes.Obtener(IdSolicitudPublicacion);
            bitacora.RegistrarActualizacion(User, "publicacion externa de solicitud", anterior, actual);
            TempData["Mensaje"] = PublicadaExterna
                ? "La solicitud se publicó para el portal externo."
                : "La solicitud se retiró del portal externo.";
        }
        catch (ArgumentException error)
        {
            if (DebeMostrarErrorEnPublicacion(error.Message))
            {
                TempData["ErrorPublicacion"] = error.Message;
                return RedirectToPage(new { publicacion = IdSolicitudPublicacion });
            }

            TempData["Error"] = error.Message;
        }

        return RedirectToPage();
    }

    // estas validaciones se muestran dentro del mismo formulario de publicacion
    private static bool DebeMostrarErrorEnPublicacion(string error) =>
        error is "Indique una fecha de vencimiento igual o posterior a hoy."
            or "No se puede publicar una solicitud que ya tiene una participacion aprobada.";
}




