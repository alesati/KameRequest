using KameRequest.Models;
using KameRequest.Repository.Repositories;
using KameRequest.Services.Abstract;

namespace KameRequest.Services;

// contiene las operaciones de tareas relacionadas con una solicitud
public class TareaService(TareaRepository repositorio) : ITareaService
{
    // lista las tareas de una solicitud por pagina
    public IReadOnlyList<TareaSolicitud> ObtenerPaginaPorSolicitud(
        int idSolicitud,
        int pagina,
        int tamano)
    {
        return repositorio.ObtenerPaginaPorSolicitud(idSolicitud, pagina, tamano);
    }

    public int ContarPorSolicitud(int idSolicitud)
    {
        return repositorio.ContarPorSolicitud(idSolicitud);
    }

    public TareaSolicitud? Obtener(int id)
    {
        return repositorio.Obtener(id);
    }

    public int Crear(TareaSolicitud tarea)
    {
        ValidarValores(tarea);
        return repositorio.Crear(tarea);
    }

    // crea una tarea despues de revisar los datos que se capturaron
    public int CrearParaSolicitud(TareaSolicitud tarea)
    {
        ValidarValores(tarea);
        return repositorio.Crear(tarea);
    }

    public void Actualizar(TareaSolicitud tarea)
    {
        ValidarValores(tarea);
        repositorio.Actualizar(tarea);
    }

    // evita actualizar tareas de otra solicitud
    public TareaSolicitud ActualizarParaSolicitud(int idSolicitud, TareaSolicitud tarea)
    {
        var anterior = repositorio.Obtener(tarea.IdTarea)
            ?? throw new ArgumentException("No se encontro la tarea solicitada.");

        if (anterior.IdSolicitud != idSolicitud)
            throw new ArgumentException("La tarea no pertenece a la solicitud indicada.");

        tarea.IdSolicitud = idSolicitud;
        ValidarValores(tarea);
        repositorio.Actualizar(tarea);
        return anterior;
    }

    public int Eliminar(int id)
    {
        return repositorio.Eliminar(id);
    }

    // no permite borrar tareas que ya estan relacionadas con un desglose
    public TareaSolicitud? EliminarDeSolicitud(int idSolicitud, int idTarea)
    {
        var tarea = repositorio.Obtener(idTarea);
        if (tarea is null || tarea.IdSolicitud != idSolicitud)
            return null;

        if (tarea.IdDesglose is not null)
            throw new InvalidOperationException("No se puede eliminar un registro con datos relacionados.");

        return repositorio.Eliminar(idTarea) == 1 ? tarea : null;
    }

    // valida los campos requeridos para una tarea de solicitud
    private static void ValidarValores(TareaSolicitud tarea)
    {
        if (string.IsNullOrWhiteSpace(tarea.Descripcion) || tarea.HorasInvertidas < 0)
            throw new ArgumentException("Digite la descripcion y las horas invertidas.");
    }
}
