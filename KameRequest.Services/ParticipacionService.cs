using KameRequest.Models;
using KameRequest.Repository.Repositories;
using KameRequest.Services.Abstract;

namespace KameRequest.Services;

// Mantiene la regla de aprobar solamente una propuesta por solicitud.
public class ParticipacionService(ParticipacionRepository repositorio) : IParticipacionService
{
    public IReadOnlyList<ParticipacionSolicitud> ListarPorSolicitud(int idSolicitud)
    {
        var participaciones = repositorio.ListarPorSolicitud(idSolicitud);
        foreach (var participacion in participaciones)
            participacion.Desgloses = repositorio.ListarDesgloses(participacion.IdParticipacion);

        return participaciones;
    }

    public bool Aprobar(int idSolicitud, int idParticipacion)
    {
        return repositorio.Aprobar(idSolicitud, idParticipacion);
    }
}