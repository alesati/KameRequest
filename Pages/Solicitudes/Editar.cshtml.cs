using Dapper;
using KameRequest.Clases;
using KameRequest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class EditarModel(
    IConfiguration configuracion,
    BitacoraRegistro bitacora,
    NotificacionesCorreos notificaciones) : PageModel
{
    [BindProperty]
    public Solicitud Solicitud { get; set; } = new();

    public IReadOnlyList<Representante> Representantes { get; private set; } = [];
    public IReadOnlyList<EstadoSolicitud> Estados { get; private set; } = [];

    public IActionResult OnGet(int id)
    {
        using var conexion = CrearConexion();
        Solicitud = ObtenerSolicitud(conexion, id) ?? new Solicitud();
        CargarCatalogos(conexion);
        return Solicitud.IdSolicitud == 0 ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPost()
    {
        using var conexion = CrearConexion();
        CargarCatalogos(conexion);
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var anterior = ObtenerSolicitud(conexion, Solicitud.IdSolicitud);
        if (anterior is null)
        {
            return NotFound();
        }

        conexion.Execute("""
            UPDATE Solicitudes
            SET ConsecutivoOficio = @ConsecutivoOficio,
                DocumentoRespuesta = @DocumentoRespuesta,
                DocumentoInicio = @DocumentoInicio,
                Titulo = @Titulo,
                Descripcion = @Descripcion,
                IdRepresentante = @IdRepresentante,
                Observaciones = @Observaciones,
                IdEstado = @IdEstado,
                FechaIngreso = @FechaIngreso,
                FechaRespuesta = @FechaRespuesta,
                FechaInicio = @FechaInicio
            WHERE IdSolicitud = @IdSolicitud;
            """, new
        {
            Solicitud.IdSolicitud,
            Solicitud.ConsecutivoOficio,
            Solicitud.DocumentoRespuesta,
            Solicitud.DocumentoInicio,
            Solicitud.Titulo,
            Descripcion = Solicitud.Descripcion ?? string.Empty,
            Solicitud.IdRepresentante,
            Solicitud.Observaciones,
            Solicitud.IdEstado,
            Solicitud.FechaIngreso,
            Solicitud.FechaRespuesta,
            Solicitud.FechaInicio
        });

        var actual = ObtenerSolicitud(conexion, Solicitud.IdSolicitud)!;
        bitacora.RegistrarActualizacion(User, "solicitud", anterior, actual);

        if ((actual.Estado ?? "") != (anterior.Estado ?? ""))
        {
            
            //se extrae el correo del representante del listado
            var representante = Representantes.FirstOrDefault(r => r.IdRepresentante == actual.IdRepresentante);

            //se extrae el nombre del estado actual y el anterior porque asi los recibe el metodo de notificacion.
            var estadoanterior = Estados.FirstOrDefault(e => e.IdEstado == anterior.IdEstado)?.Nombre ?? anterior.Estado ?? "Desconocido";
            var estadonuevo = Estados.FirstOrDefault(e => e.IdEstado == actual.IdEstado)?.Nombre ?? actual.Estado ?? "Desconocido";
            if (!string.IsNullOrWhiteSpace(representante?.Email))
            {
                notificaciones.EnviarCorreoCambioEstadoManual(User, representante.Email , estadoanterior, estadonuevo, Solicitud.Consecutivo);
            }
        }

        TempData["Mensaje"] = "La solicitud se actualizo correctamente.";
        return RedirectToPage("Index");
    }

    private void CargarCatalogos(MySqlConnection conexion)
    {
        Representantes = conexion.Query<Representante>("""
            SELECT IdRepresentante, Nombre, Email
            FROM Representantes
            ORDER BY Nombre;
            """).ToList();
        Estados = conexion.Query<EstadoSolicitud>("""
            SELECT IdEstado, Nombre
            FROM EstadosSolicitud
            ORDER BY IdEstado;
            """).ToList();
    }

    private static Solicitud? ObtenerSolicitud(MySqlConnection conexion, int idSolicitud) =>
        conexion.QuerySingleOrDefault<Solicitud>("""
            SELECT s.IdSolicitud, s.ConsecutivoOficio, s.DocumentoRespuesta,
                   s.DocumentoInicio, s.Titulo, s.Descripcion, s.IdRepresentante,
                   s.Observaciones, s.IdEstado, s.FechaIngreso, s.FechaRespuesta,
                   s.FechaInicio, e.Nombre AS Estado, r.Nombre AS Representante
            FROM Solicitudes s
            INNER JOIN EstadosSolicitud e ON e.IdEstado = s.IdEstado
            LEFT JOIN Representantes r ON r.IdRepresentante = s.IdRepresentante
            WHERE s.IdSolicitud = @idSolicitud;
            """, new { idSolicitud });

    private MySqlConnection CrearConexion() =>
        new(configuracion.GetConnectionString("MySql"));
}
