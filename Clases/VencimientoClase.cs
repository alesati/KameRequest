// reglas que se usan en esta parte del sistema
using System.Text.Json;
using KameRequest.Models;
using KameRequest.Repository.Repositories;

namespace KameRequest.Clases;

public class VencimientoClase(
    SolicitudRepository solicitudes,
    BitacoraRegistro bitacora,
    NotificacionesCorreos notificaciones)
{
    // revisa las solicitudes pendientes y marca las que ya vencieron
    public void ActualizarSolicitudesVencidas()
    {
        var idEstado = solicitudes.ObtenerIdEstadoPorNombre("Vencida");
        if (!idEstado.HasValue)
            throw new InvalidOperationException("No existe el estado Vencida para las solicitudes.");

        foreach (var solicitud in solicitudes.SinRespuestaNoVencidas(idEstado.Value).Where(s => AgregarDiasHabiles(s.FechaIngreso, 5) <= DateTime.Today))
        {
            var anterior = CopiarSolicitud(solicitud);
            if (solicitudes.MarcarVencida(solicitud.IdSolicitud, idEstado.Value) != 1)
                continue;

            solicitud.IdEstado = idEstado.Value;
            solicitud.Estado = "Vencida";
            bitacora.RegistrarSistema($"Actualizo solicitud por vencimiento. Anterior: {JsonSerializer.Serialize(anterior)}. Actual: {JsonSerializer.Serialize(solicitud)}");
            notificaciones.EnviarCorreoCambioEstadoAuto(solicitud.IdSolicitud);
        }
    }

    private static DateTime AgregarDiasHabiles(DateTime fechaIngreso, int cantidadDias)
    {
        var fecha = fechaIngreso.Date;
        var agregados = 0;
        while (agregados < cantidadDias)
        {
            fecha = fecha.AddDays(1);
            if (fecha.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                agregados++;
        }
        return fecha;
    }

    private static Solicitud CopiarSolicitud(Solicitud s) => new()
    {
        IdSolicitud = s.IdSolicitud,
        ConsecutivoOficio = s.ConsecutivoOficio,
        DocumentoRespuesta = s.DocumentoRespuesta,
        DocumentoInicio = s.DocumentoInicio,
        Titulo = s.Titulo,
        Descripcion = s.Descripcion,
        IdRepresentante = s.IdRepresentante,
        Observaciones = s.Observaciones,
        IdEstado = s.IdEstado,
        FechaIngreso = s.FechaIngreso,
        FechaRespuesta = s.FechaRespuesta,
        FechaInicio = s.FechaInicio,
        Estado = s.Estado,
        Representante = s.Representante
    };
}

