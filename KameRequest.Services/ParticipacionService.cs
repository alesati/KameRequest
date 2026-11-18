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

    // valida que la propuesta sea de la solicitud antes de convertirla en desglose oficial
    public ParticipacionSolicitud AprobarParaSolicitud(int idSolicitud, int idParticipacion)
    {
        var anterior = repositorio.ListarPorSolicitud(idSolicitud)
            .SingleOrDefault(item => item.IdParticipacion == idParticipacion)
            ?? throw new ArgumentException("No se encontro la participacion solicitada.");

        if (!repositorio.Aprobar(idSolicitud, idParticipacion))
        {
            throw new InvalidOperationException(
                "Ya existe una propuesta aprobada o la participación no corresponde a la solicitud.");
        }

        return anterior;
    }
}
