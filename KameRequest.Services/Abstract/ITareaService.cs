using KameRequest.Models;

namespace KameRequest.Services.Abstract;

// contrato para las tareas que se registran dentro de una solicitud
public interface ITareaService
{
    IReadOnlyList<TareaSolicitud> ObtenerPaginaPorSolicitud(int idSolicitud, int pagina, int tamano);
    int ContarPorSolicitud(int idSolicitud);
    TareaSolicitud? Obtener(int id);
    int Crear(TareaSolicitud tarea);
    void Actualizar(TareaSolicitud tarea);
    int Eliminar(int id);
}
