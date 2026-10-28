using KameRequest.Models;

namespace KameRequest.Services.Abstract;

public interface IParticipacionService
{
    IReadOnlyList<ParticipacionSolicitud> ListarPorSolicitud(int idSolicitud);
    bool Aprobar(int idSolicitud, int idParticipacion);
}