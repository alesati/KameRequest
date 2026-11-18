using KameRequest.Models;
using KameRequest.Repository.Repositories;
using KameRequest.Services.Abstract;
using MySqlConnector;
using System.Security.Claims;

namespace KameRequest.Services;

// concentra las operaciones que pertenecen al modulo de solicitudes
public class SolicitudService(
    SolicitudRepository repositorio,
    ParticipacionRepository participaciones,
    RepresentanteRepository representantes,
    INotificacionesCorreoService notificaciones) : ISolicitudService
{
    public Solicitud? Obtener(int id)
    {
        return repositorio.Obtener(id);
    }

    // trae las solicitudes necesarias para la pagina actual
    public IReadOnlyList<Solicitud> ObtenerPagina(int pagina, int tamano)
    {
        return repositorio.ObtenerPagina(pagina, tamano);
    }

    public int Contar()
    {
        return repositorio.Contar();
    }

    public int Crear(Solicitud solicitud)
    {
        try
        {
            return repositorio.Crear(solicitud);
        }
        catch (MySqlException error) when (error.Number == 1062)
        {
            throw new ArgumentException("El consecutivo de oficio ya existe.");
        }
    }

    public void Actualizar(Solicitud solicitud)
    {
        repositorio.Actualizar(solicitud);
    }

    // guarda la solicitud y notifica solo cuando realmente cambio el estado
    public (Solicitud Anterior, Solicitud Actual) ActualizarConNotificacion(
        ClaimsPrincipal usuario,
        Solicitud solicitud)
    {
        var anterior = repositorio.Obtener(solicitud.IdSolicitud)
            ?? throw new ArgumentException("No se encontro la solicitud.");

        repositorio.Actualizar(solicitud);
        var actual = repositorio.Obtener(solicitud.IdSolicitud)
            ?? throw new InvalidOperationException("No se pudo recuperar la solicitud actualizada.");

        if (actual.IdEstado != anterior.IdEstado)
        {
            var representante = actual.IdRepresentante.HasValue
                ? representantes.Obtener(actual.IdRepresentante.Value)
                : null;
            notificaciones.EnviarCorreoCambioEstadoManual(
                usuario,
                representante?.Email,
                anterior.Estado ?? "Desconocido",
                actual.Estado ?? "Desconocido",
                actual.ConsecutivoOficio);
        }

        return (anterior, actual);
    }

    // revisa las reglas antes de hacer visible una solicitud en el portal
    public void ActualizarPublicacionExterna(int idSolicitud, bool publicada, DateTime? fechaVencimiento)
    {
        if (!publicada)
        {
            repositorio.ActualizarPublicacionExterna(idSolicitud, false, null);
            return;
        }

        var solicitud = repositorio.Obtener(idSolicitud)
            ?? throw new ArgumentException("No se encontro la solicitud.");

        if (!string.Equals(solicitud.Estado, "Nueva", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Solo se puede publicar una solicitud que este en estado Nueva.");

        if (participaciones.TieneAprobada(idSolicitud))
            throw new ArgumentException("No se puede publicar una solicitud que ya tiene una participacion aprobada.");

        if (!fechaVencimiento.HasValue || fechaVencimiento.Value.Date < DateTime.Today)
            throw new ArgumentException("Indique una fecha de vencimiento igual o posterior a hoy.");

        repositorio.ActualizarPublicacionExterna(idSolicitud, true, fechaVencimiento);
    }

    public int Eliminar(int id)
    {
        return repositorio.Eliminar(id);
    }

    // valida las relaciones antes de eliminar una solicitud
    public Solicitud? EliminarSeguro(int id)
    {
        var solicitud = repositorio.Obtener(id);
        if (solicitud is null)
            return null;

        try
        {
            return repositorio.Eliminar(id) == 1 ? solicitud : null;
        }
        catch (MySqlException error) when (error.Number == 1451)
        {
            throw new InvalidOperationException("No se puede eliminar un registro con datos relacionados.");
        }
    }

    // calcula dias habiles para reutilizar la misma regla fuera de la pagina
    public static DateTime AgregarDiasHabiles(DateTime fechaIngreso, int cantidadDias)
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

    // los estados se usan al crear o editar una solicitud
    public IReadOnlyList<EstadoSolicitud> ListarEstados()
    {
        return repositorio.ListarEstados();
    }

    // se usa en el proceso automatico para revisar solo solicitudes nuevas
    public IReadOnlyList<Solicitud> NuevasSinRespuesta(int idEstadoNueva)
    {
        return repositorio.NuevasSinRespuesta(idEstadoNueva);
    }

    public int? ObtenerIdEstadoPorNombre(string nombre)
    {
        return repositorio.ObtenerIdEstadoPorNombre(nombre);
    }

    public int MarcarVencida(int idSolicitud, int idEstadoNueva, int idEstadoVencida)
    {
        return repositorio.MarcarVencida(idSolicitud, idEstadoNueva, idEstadoVencida);
    }

    public IReadOnlyList<Solicitud> ListarParaReporte()
    {
        return repositorio.ListarParaReporte();
    }
}

