using System.Text.Json;
using KameRequest.Models;
using KameRequest.Repository.Repositories;

namespace KameRequest.Clases;

// revisa las solicitudes que se vencen sin respuesta
public class VencimientoClase(
    SolicitudRepository solicitudes,
    BitacoraRegistro bitacora,
    NotificacionesCorreos notificaciones)
{
    // busca las solicitudes atrasadas y cambia su estado a Vencida
    public void ActualizarSolicitudesVencidas()
    {
        // se busca el id para no escribir un numero fijo en el codigo
        var idEstado = solicitudes.ObtenerIdEstadoPorNombre("Vencida");
        if (!idEstado.HasValue)
            throw new InvalidOperationException("No existe el estado Vencida para las solicitudes.");

        // solo revisa solicitudes pendientes que ya cumplen los 5 dias habiles
        foreach (var solicitud in solicitudes
                     .SinRespuestaNoVencidas(idEstado.Value)
                     .Where(s => AgregarDiasHabiles(s.FechaIngreso, 5) <= DateTime.Today))
        {
            // se guarda una copia para mostrar el antes y despues en bitacora
            var anterior = CopiarSolicitud(solicitud);

            // actualiza la base de datos y sigue solo si se modifico una solicitud
            if (solicitudes.MarcarVencida(solicitud.IdSolicitud, idEstado.Value) != 1)
                continue;

            // actualiza los datos del objeto que esta en memoria
            solicitud.IdEstado = idEstado.Value;
            solicitud.Estado = "Vencida";

            // como lo hace el sistema se registra sin un usuario especifico
            bitacora.RegistrarSistema($"Actualizo solicitud por vencimiento. Anterior: {JsonSerializer.Serialize(anterior)}. Actual: {JsonSerializer.Serialize(solicitud)}");

            // avisa al representante que cambio el estado de su solicitud
            notificaciones.EnviarCorreoCambioEstadoAuto(solicitud.IdSolicitud);
        }
    }

    // suma dias sin contar sabados ni domingos
    private static DateTime AgregarDiasHabiles(DateTime fechaIngreso, int cantidadDias)
    {
        var fecha = fechaIngreso.Date;
        var agregados = 0;

        // avanza un dia a la vez para validar si es un dia laboral
        while (agregados < cantidadDias)
        {
            fecha = fecha.AddDays(1);

            if (fecha.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                agregados++;
        }

        return fecha;
    }

    // crea otra solicitud para conservar los datos antes del cambio
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
