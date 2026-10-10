using System.Text.Json;
using Dapper;
using KameRequest.Models;
using MySqlConnector;

namespace KameRequest.Clases;

// Actualiza solicitudes sin respuesta cuando ya cumplieron cinco dias habiles.
public class VencimientoClase(IConfiguration configuracion, BitacoraRegistro bitacora,    NotificacionesCorreos notificaciones)
{
    public void ActualizarSolicitudesVencidas()
    {
        using var conexion = CrearConexion();
        var idEstadoVencida = conexion.QuerySingleOrDefault<int?>("""
            SELECT IdEstado
            FROM EstadosSolicitud
            WHERE Nombre = 'Vencida'
            LIMIT 1;
            """);

        if (!idEstadoVencida.HasValue)
        {
            throw new InvalidOperationException("No existe el estado Vencida para las solicitudes.");
        }

        var solicitudes = conexion.Query<Solicitud>("""
            SELECT s.IdSolicitud, s.ConsecutivoOficio, s.DocumentoRespuesta,
                   s.DocumentoInicio, s.Titulo, s.Descripcion, s.IdRepresentante,
                   s.Observaciones, s.IdEstado, s.FechaIngreso, s.FechaRespuesta,
                   s.FechaInicio, e.Nombre AS Estado, r.Nombre AS Representante
            FROM Solicitudes s
            INNER JOIN EstadosSolicitud e ON e.IdEstado = s.IdEstado
            LEFT JOIN Representantes r ON r.IdRepresentante = s.IdRepresentante
            WHERE s.FechaRespuesta IS NULL
              AND s.IdEstado <> @idEstadoVencida;
            """, new { idEstadoVencida })
            .Where(solicitud => AgregarDiasHabiles(solicitud.FechaIngreso, 5) <= DateTime.Today)
            .ToList();
        
        foreach (var solicitud in solicitudes)
        {
            var anterior = CopiarSolicitud(solicitud);
            var actualizadas = conexion.Execute("""
                UPDATE Solicitudes
                SET IdEstado = @idEstadoVencida
                WHERE IdSolicitud = @idSolicitud
                  AND FechaRespuesta IS NULL
                  AND IdEstado <> @idEstadoVencida;
                """, new { idEstadoVencida, idSolicitud = solicitud.IdSolicitud });

            if (actualizadas != 1)
            {
                continue;
            }

            solicitud.IdEstado = idEstadoVencida.Value;
            solicitud.Estado = "Vencida";
            bitacora.RegistrarSistema(
                $"Actualizo solicitud por vencimiento. Anterior: {JsonSerializer.Serialize(anterior)}. " +
                $"Actual: {JsonSerializer.Serialize(solicitud)}");

            //notifica por correo el cambio automatico
           notificaciones.EnviarCorreoCambioEstadoAuto(solicitud.IdSolicitud);
            
        }
    }

    // Suma dias de lunes a viernes para usar el mismo criterio del listado.
    private static DateTime AgregarDiasHabiles(DateTime fechaIngreso, int cantidadDias)
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
    private static Solicitud CopiarSolicitud(Solicitud solicitud) => new()
    {
        IdSolicitud = solicitud.IdSolicitud,
        ConsecutivoOficio = solicitud.ConsecutivoOficio,
        DocumentoRespuesta = solicitud.DocumentoRespuesta,
        DocumentoInicio = solicitud.DocumentoInicio,
        Titulo = solicitud.Titulo,
        Descripcion = solicitud.Descripcion,
        IdRepresentante = solicitud.IdRepresentante,
        Observaciones = solicitud.Observaciones,
        IdEstado = solicitud.IdEstado,
        FechaIngreso = solicitud.FechaIngreso,
        FechaRespuesta = solicitud.FechaRespuesta,
        FechaInicio = solicitud.FechaInicio,
        Estado = solicitud.Estado,
        Representante = solicitud.Representante
    };

    private MySqlConnection CrearConexion() =>
        new(configuracion.GetConnectionString("MySql"));
}
