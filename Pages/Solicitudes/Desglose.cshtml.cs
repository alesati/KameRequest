using Dapper;
using KameRequest.Models;
using KameRequest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Solicitudes;

[Authorize]
public class DesgloseModel(IConfiguration configuracion, BitacoraService bitacora) : PageModel
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

    public IActionResult OnGet(int id)
    {
        return CargarDatos(id) ? Page() : RedirectToPage("/Solicitudes/Index");
    }

    public IActionResult OnPostAgregar(int idSolicitud)
    {
        if (!DatosValidos(NuevoDesglose))
        {
            MensajeError = "Revise el mes, año, monto y porcentaje de cobro.";
            return RedirectToPage(new { id = idSolicitud });
        }

        using var conexion = CrearConexion();
        NuevoDesglose.IdSolicitud = idSolicitud;
        conexion.Execute("""
            INSERT INTO DesglosesSolicitud
                (IdSolicitud, Mes, Anio, Horas, Monto, PorcentajeCobro, Observaciones)
            VALUES
                (@IdSolicitud, @Mes, @Anio, @Horas, @Monto, @PorcentajeCobro, @Observaciones);
            """, NuevoDesglose);

        NuevoDesglose.IdDesglose = conexion.ExecuteScalar<int>("SELECT LAST_INSERT_ID();");
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

        using var conexion = CrearConexion();
        var anterior = ObtenerDesglose(conexion, DesgloseEditar.IdDesglose);
        if (anterior is null || anterior.IdSolicitud != idSolicitud)
        {
            return RedirectToPage(new { id = idSolicitud });
        }

        conexion.Execute("""
            UPDATE DesglosesSolicitud
            SET Mes = @Mes, Anio = @Anio, Horas = @Horas, Monto = @Monto,
                PorcentajeCobro = @PorcentajeCobro, Observaciones = @Observaciones
            WHERE IdDesglose = @IdDesglose;
            """, DesgloseEditar);

        var actual = ObtenerDesglose(conexion, DesgloseEditar.IdDesglose);
        bitacora.RegistrarActualizacion(User, "desglose", anterior, actual);
        return RedirectToPage(new { id = idSolicitud });
    }

    public IActionResult OnPostEliminar(int idDesglose, int idSolicitud)
    {
        using var conexion = CrearConexion();
        var eliminado = ObtenerDesglose(conexion, idDesglose);
        if (eliminado is null || eliminado.IdSolicitud != idSolicitud)
        {
            return RedirectToPage(new { id = idSolicitud });
        }

        try
        {
            conexion.Execute("DELETE FROM DesglosesSolicitud WHERE IdDesglose = @idDesglose", new { idDesglose });
            bitacora.RegistrarEliminacion(User, "desglose", eliminado);
        }
        catch (MySqlException excepcion) when (excepcion.Number == 1451)
        {
            MensajeError = "No se puede eliminar un registro con datos relacionados.";
        }

        return RedirectToPage(new { id = idSolicitud });
    }

    private bool CargarDatos(int idSolicitud)
    {
        using var conexion = CrearConexion();
        SolicitudActual = conexion.QuerySingleOrDefault<Solicitud>("""
            SELECT s.IdSolicitud, s.ConsecutivoOficio, s.Titulo, s.Descripcion,
                   s.IdRepresentante, s.IdEstado, s.FechaIngreso, s.FechaRespuesta, s.FechaInicio,
                   COALESCE(r.Nombre, 'Sin asignar') AS Representante,
                   COALESCE(e.Nombre, 'Sin estado') AS Estado
            FROM Solicitudes s
            LEFT JOIN Representantes r ON r.IdRepresentante = s.IdRepresentante
            LEFT JOIN EstadosSolicitud e ON e.IdEstado = s.IdEstado
            WHERE s.IdSolicitud = @idSolicitud;
            """, new { idSolicitud });

        if (SolicitudActual is null)
        {
            return false;
        }

        ListaDesgloses = conexion.Query<DesgloseSolicitud>("""
            SELECT IdDesglose, IdSolicitud, Mes, Anio, Horas, Monto, PorcentajeCobro, Observaciones
            FROM DesglosesSolicitud
            WHERE IdSolicitud = @idSolicitud
            ORDER BY Anio, Mes, IdDesglose;
            """, new { idSolicitud }).ToList();
        return true;
    }

    private static DesgloseSolicitud? ObtenerDesglose(MySqlConnection conexion, int idDesglose) =>
        conexion.QuerySingleOrDefault<DesgloseSolicitud>("""
            SELECT IdDesglose, IdSolicitud, Mes, Anio, Horas, Monto, PorcentajeCobro, Observaciones
            FROM DesglosesSolicitud
            WHERE IdDesglose = @idDesglose;
            """, new { idDesglose });

    private static bool DatosValidos(DesgloseSolicitud datos) =>
        datos.Mes is >= 1 and <= 12 && datos.Anio is >= 1000 and <= 9999 &&
        datos.Horas >= 0 && datos.Monto >= 0 && datos.PorcentajeCobro is >= 0 and <= 100;

    private MySqlConnection CrearConexion() =>
        new(configuracion.GetConnectionString("MySql"));
}
