using Dapper;
using KameRequest.Models;
using KameRequest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class IndexModel(IConfiguration configuracion, BitacoraService bitacora) : PageModel
{
    private const int RegistrosPorPagina = 5;

    public IReadOnlyList<Solicitud> Solicitudes { get; private set; } = [];
    public int PaginaActual { get; private set; }
    public int TotalPaginas { get; private set; }

    public void OnGet(int pagina = 1)
    {
        bitacora.RegistrarConsulta(User, "solicitudes");
        CargarSolicitudes(pagina);
    }

    public IActionResult OnPostEliminar(int idSolicitud)
    {
        try
        {
            using var conexion = CrearConexion();
            var eliminada = ObtenerSolicitud(conexion, idSolicitud);
            if (eliminada is null)
            {
                return RedirectToPage();
            }

            var eliminados = conexion.Execute(
                "DELETE FROM Solicitudes WHERE IdSolicitud = @idSolicitud;",
                new { idSolicitud });

            if (eliminados == 1)
            {
                bitacora.RegistrarEliminacion(User, "solicitud", eliminada);
                TempData["Mensaje"] = "La solicitud se elimino correctamente.";
            }
        }
        catch (MySqlException excepcion) when (excepcion.Number == 1451)
        {
            TempData["Error"] = "No se puede eliminar un registro con datos relacionados.";
        }

        return RedirectToPage();
    }

    // Calcula los cinco dias habiles solicitados, sin contar sabados ni domingos.
    public static DateTime AgregarDiasHabiles(DateTime fechaIngreso, int cantidadDias)
    {
        var fecha = fechaIngreso.Date;
        var diasAgregados = 0;

        while (diasAgregados < cantidadDias)
        {
            fecha = fecha.AddDays(1);
            if (fecha.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            {
                diasAgregados++;
            }
        }

        return fecha;
    }

    private void CargarSolicitudes(int pagina)
    {
        using var conexion = CrearConexion();
        var totalRegistros = conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Solicitudes;");
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)RegistrosPorPagina));
        PaginaActual = Math.Clamp(pagina, 1, TotalPaginas);

        Solicitudes = conexion.Query<Solicitud>("""
            SELECT s.IdSolicitud, s.ConsecutivoOficio, s.Titulo, s.IdEstado,
                   s.FechaIngreso, e.Nombre AS Estado,
                   r.Nombre AS Representante
            FROM Solicitudes s
            INNER JOIN EstadosSolicitud e ON e.IdEstado = s.IdEstado
            LEFT JOIN Representantes r ON r.IdRepresentante = s.IdRepresentante
            ORDER BY s.FechaIngreso DESC, s.IdSolicitud DESC
            LIMIT @registrosPorPagina OFFSET @desplazamiento;
            """, new
        {
            registrosPorPagina = RegistrosPorPagina,
            desplazamiento = (PaginaActual - 1) * RegistrosPorPagina
        }).ToList();
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
