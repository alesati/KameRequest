using KameRequest.Models;
using KameRequest.Repository.Repositories;
using KameRequest.Services.Abstract;
using MySqlConnector;

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
        var anterior = repositorio.Obtener(estado.idEstado)
            ?? throw new ArgumentException("No se encontro el estado solicitado.");

        // el estado inicial conserva el nombre para crear solicitudes nuevas
        if (estado.idEstado == 1 && !string.Equals(estado.Nombre, anterior.Nombre, StringComparison.Ordinal))
            throw new ArgumentException("No se puede cambiar el nombre del estado inicial.");

        repositorio.Actualizar(estado);
    }

    public int Eliminar(int idEstado)
    {
        return repositorio.Eliminar(idEstado);
    }

    // evita eliminar estados que ya son usados por solicitudes
    public Estado? EliminarSeguro(int idEstado)
    {
        var estado = repositorio.Obtener(idEstado);
        if (estado is null)
            return null;

        try
        {
            return repositorio.Eliminar(idEstado) == 1 ? estado : null;
        }
        catch (MySqlException error) when (error.Number == 1451)
        {
            throw new InvalidOperationException("No se puede eliminar un registro con datos relacionados.");
        }
    }
}
