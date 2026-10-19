using KameRequest.Models;

namespace KameRequest.Services.Abstract;

// contrato para las operaciones principales del modulo de solicitudes
public interface ISolicitudService
{
    Solicitud? Obtener(int id);
    IReadOnlyList<Solicitud> ObtenerPagina(int pagina, int tamano);
    int Contar();
    int Crear(Solicitud solicitud);
    void Actualizar(Solicitud solicitud);
    int Eliminar(int id);
    IReadOnlyList<EstadoSolicitud> ListarEstados();
    IReadOnlyList<Solicitud> NuevasSinRespuesta(int idEstadoNueva);
    int? ObtenerIdEstadoPorNombre(string nombre);
    int MarcarVencida(int idSolicitud, int idEstadoNueva, int idEstadoVencida);
    IReadOnlyList<Solicitud> ListarParaReporte();
}

