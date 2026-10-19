using KameRequest.Models;

namespace KameRequest.Services.Abstract;

// contrato para administrar los porcentajes de cobro de una solicitud
public interface IDesgloseService
{
    IReadOnlyList<DesgloseSolicitud> ObtenerPaginaPorSolicitud(int idSolicitud, int pagina, int tamano);
    int ContarPorSolicitud(int idSolicitud);
    decimal SumarPorcentajePorSolicitud(int idSolicitud, int? excluirIdDesglose = null);
    DesgloseSolicitud? Obtener(int id);
    int Crear(DesgloseSolicitud desglose);
    void Actualizar(DesgloseSolicitud desglose);
    int Eliminar(int id);
}
