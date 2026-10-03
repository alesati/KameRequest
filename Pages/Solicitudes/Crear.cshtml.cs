using Dapper;
using KameRequest.Models;
using KameRequest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class CrearModel(
    IConfiguration configuracion,
    BitacoraService bitacora) : PageModel
{
    [BindProperty]
    public Solicitud Solicitud { get; set; } = new();

    public IReadOnlyList<Representante> Representantes { get; private set; } = [];

    public void OnGet()
    {
        Solicitud.FechaIngreso = DateTime.Today;
        CargarRepresentantes();
    }

    public IActionResult OnPost()
    {
        CargarRepresentantes();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        using var conexion = CrearConexion();
        var idEstadoNueva = conexion.ExecuteScalar<int>("""
            SELECT IdEstado FROM EstadosSolicitud
            WHERE Nombre = 'Nueva'
            LIMIT 1;
            """);

        conexion.Execute("""
            INSERT INTO Solicitudes
                (ConsecutivoOficio, DocumentoRespuesta, DocumentoInicio, Titulo,
                 MontoTotal, Descripcion, IdRepresentante, Observaciones,
                 IdEstado, FechaIngreso, FechaRespuesta, FechaInicio)
            VALUES
                (@ConsecutivoOficio, @DocumentoRespuesta, @DocumentoInicio, @Titulo,
                 0, @Descripcion, @IdRepresentante, @Observaciones,
                 @idEstadoNueva, @FechaIngreso, @FechaRespuesta, @FechaInicio);
            """, new
        {
            Solicitud.ConsecutivoOficio,
            Solicitud.DocumentoRespuesta,
            Solicitud.DocumentoInicio,
            Solicitud.Titulo,
            Descripcion = Solicitud.Descripcion ?? string.Empty,
            Solicitud.IdRepresentante,
            Solicitud.Observaciones,
            idEstadoNueva,
            Solicitud.FechaIngreso,
            Solicitud.FechaRespuesta,
            Solicitud.FechaInicio
        });

        Solicitud.IdSolicitud = conexion.ExecuteScalar<int>("SELECT LAST_INSERT_ID();");
        var nueva = ObtenerSolicitud(conexion, Solicitud.IdSolicitud)!;
        bitacora.RegistrarCreacion(User, "solicitud", nueva);

        TempData["Mensaje"] = "La solicitud se creo correctamente.";
        return RedirectToPage("Index");
    }

    private void CargarRepresentantes()
    {
        using var conexion = CrearConexion();
        Representantes = conexion.Query<Representante>("""
            SELECT IdRepresentante, Nombre, Email
            FROM Representantes
            ORDER BY Nombre;
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
