using KameRequest.Models;
using KameRequest.Repository.Repositories;
using KameRequest.Services.Abstract;

namespace KameRequest.Services;

// contiene las operaciones del mantenimiento de representantes
public class RepresentanteService(RepresentanteRepository repositorio) : IRepresentanteService
{
    public IReadOnlyList<Representante> Listar()
    {
        return repositorio.Listar();
    }

    public IReadOnlyList<Representante> ObtenerPagina(int pagina, int tamano)
    {
        return repositorio.ObtenerPagina(pagina, tamano);
    }

    public int Contar()
    {
        return repositorio.Contar();
    }

    public Representante? Obtener(int id)
    {
        return repositorio.Obtener(id);
    }

    public int Crear(Representante representante)
    {
        return repositorio.Crear(representante);
    }

    public void Actualizar(Representante representante)
    {
        repositorio.Actualizar(representante);
    }

    public int Eliminar(int id)
    {
        return repositorio.Eliminar(id);
    }
}
