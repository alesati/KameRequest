using KameRequest.Models;
using KameRequest.Repository.Repositories;
using KameRequest.Services.Abstract;

namespace KameRequest.Services;

// permite que las pantallas usen estados sin llamar al repositorio directamente
public class EstadoService(EstadoRepository repositorio) : IEstadoService
{
    // trae solo los estados de la pagina actual
    public IReadOnlyList<Estado> ObtenerPagina(int pagina, int tamano)
    {
        return repositorio.ObtenerPagina(pagina, tamano);
    }

    public int Contar()
    {
        return repositorio.Contar();
    }

    public Estado? Obtener(int idEstado)
    {
        return repositorio.Obtener(idEstado);
    }

    public int Crear(Estado estado)
    {
        return repositorio.Crear(estado);
    }

    public void Actualizar(Estado estado)
    {
        repositorio.Actualizar(estado);
    }

    public int Eliminar(int idEstado)
    {
        return repositorio.Eliminar(idEstado);
    }
}
