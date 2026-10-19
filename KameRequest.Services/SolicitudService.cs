using KameRequest.Models;
using KameRequest.Repository.Repositories;
using KameRequest.Services.Abstract;

namespace KameRequest.Services;

// concentra las operaciones que pertenecen al modulo de solicitudes
public class SolicitudService(SolicitudRepository repositorio) : ISolicitudService
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
        return repositorio.Crear(solicitud);
    }

    public void Actualizar(Solicitud solicitud)
    {
        repositorio.Actualizar(solicitud);
    }

    public int Eliminar(int id)
    {
        return repositorio.Eliminar(id);
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

