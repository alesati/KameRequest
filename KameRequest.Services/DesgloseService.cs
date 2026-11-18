using KameRequest.Models;
using KameRequest.Repository.Repositories;
using KameRequest.Services.Abstract;
using MySqlConnector;

namespace KameRequest.Services;

// maneja los datos de cobro mensual de cada solicitud
public class DesgloseService(DesgloseRepository repositorio) : IDesgloseService
{
    // trae los desgloses de una solicitud por pagina
    public IReadOnlyList<DesgloseSolicitud> ObtenerPaginaPorSolicitud(
        int idSolicitud,
        int pagina,
        int tamano)
    {
        return repositorio.ObtenerPaginaPorSolicitud(idSolicitud, pagina, tamano);
    }

    // cuenta los desgloses para formar la paginacion
    public int ContarPorSolicitud(int idSolicitud)
    {
        return repositorio.ContarPorSolicitud(idSolicitud);
    }

    // se usa para validar que los porcentajes no pasen de 100
    public decimal SumarPorcentajePorSolicitud(int idSolicitud, int? excluirIdDesglose = null)
    {
        return repositorio.SumarPorcentajePorSolicitud(idSolicitud, excluirIdDesglose);
    }

    public DesgloseSolicitud? Obtener(int id)
    {
        return repositorio.Obtener(id);
    }

    public int Crear(DesgloseSolicitud desglose)
    {
        ValidarValores(desglose);
        return repositorio.Crear(desglose);
    }

    // aplica todas las reglas antes de crear un periodo de cobro
    public int CrearParaSolicitud(int idSolicitud, DesgloseSolicitud desglose)
    {
        desglose.IdSolicitud = idSolicitud;
        ValidarValores(desglose);
        ValidarPorcentajeDisponible(idSolicitud, desglose.PorcentajeCobro);
        return repositorio.Crear(desglose);
    }

    public void Actualizar(DesgloseSolicitud desglose)
    {
        ValidarValores(desglose);
        repositorio.Actualizar(desglose);
    }

    // confirma que el desglose pertenece a la solicitud antes de actualizarlo
    public DesgloseSolicitud ActualizarParaSolicitud(int idSolicitud, DesgloseSolicitud desglose)
    {
        var anterior = repositorio.Obtener(desglose.IdDesglose)
            ?? throw new ArgumentException("No se encontro el desglose solicitado.");

        if (anterior.IdSolicitud != idSolicitud)
            throw new ArgumentException("El desglose no pertenece a la solicitud indicada.");

        desglose.IdSolicitud = idSolicitud;
        ValidarValores(desglose);
        ValidarPorcentajeDisponible(idSolicitud, desglose.PorcentajeCobro, desglose.IdDesglose);
        repositorio.Actualizar(desglose);
        return anterior;
    }

    // evita guardar valores en cero aunque la llamada no venga desde la pantalla
    private static void ValidarValores(DesgloseSolicitud desglose)
    {
        if (desglose.Mes is < 1 or > 12 || desglose.Anio is < 1000 or > 9999
            || desglose.Horas <= 0 || desglose.Monto <= 0 || desglose.PorcentajeCobro is <= 0 or > 100)
            throw new ArgumentException("Las horas, el monto y el porcentaje de cobro deben ser mayores a 0.");
    }

    // no permite que la suma de los periodos de cobro supere 100 por ciento
    private void ValidarPorcentajeDisponible(int idSolicitud, decimal porcentaje, int? excluirIdDesglose = null)
    {
        if (repositorio.SumarPorcentajePorSolicitud(idSolicitud, excluirIdDesglose) + porcentaje > 100)
            throw new ArgumentException("El porcentaje total de cobro no puede ser mayor a 100%.");
    }

    public int Eliminar(int id)
    {
        return repositorio.Eliminar(id);
    }

    // elimina solo un desglose de la solicitud actual y traduce la restriccion de base
    public DesgloseSolicitud? EliminarDeSolicitud(int idSolicitud, int idDesglose)
    {
        var desglose = repositorio.Obtener(idDesglose);
        if (desglose is null || desglose.IdSolicitud != idSolicitud)
            return null;

        try
        {
            return repositorio.Eliminar(idDesglose) == 1 ? desglose : null;
        }
        catch (MySqlException error) when (error.Number == 1451)
        {
            throw new InvalidOperationException("No se puede eliminar un registro con datos relacionados.");
        }
    }
}
