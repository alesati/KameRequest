using KameRequest.Models;
using KameRequest.Repository.Repositories;
using KameRequest.Services.Abstract;
using MySqlConnector;

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

    // valida que el representante no tenga solicitudes relacionadas
    public Representante? EliminarSeguro(int id)
    {
        var representante = repositorio.Obtener(id);
        if (representante is null)
            return null;

        try
        {
            return repositorio.Eliminar(id) == 1 ? representante : null;
        }
        catch (MySqlException error) when (error.Number == 1451)
        {
            throw new InvalidOperationException("No se puede eliminar un registro con datos relacionados.");
        }
    }
}
