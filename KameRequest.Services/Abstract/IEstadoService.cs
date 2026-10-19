using KameRequest.Models;

namespace KameRequest.Services.Abstract;

// contrato para el mantenimiento de estados de solicitud
public interface IEstadoService
{
    IReadOnlyList<Estado> ObtenerPagina(int pagina, int tamano);
    int Contar();
    Estado? Obtener(int idEstado);
    int Crear(Estado estado);
    void Actualizar(Estado estado);
    int Eliminar(int idEstado);
}
