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
        return repositorio.Crear(tarea);
    }

    public void Actualizar(TareaSolicitud tarea)
    {
        repositorio.Actualizar(tarea);
    }

    public int Eliminar(int id)
    {
        return repositorio.Eliminar(id);
    }
}
