using KameRequest.Models;
using KameRequest.Repository.Repositories;
using KameRequest.Services.Abstract;

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

    public void Actualizar(DesgloseSolicitud desglose)
    {
        ValidarValores(desglose);
        repositorio.Actualizar(desglose);
    }

    // evita guardar valores en cero aunque la llamada no venga desde la pantalla
    private static void ValidarValores(DesgloseSolicitud desglose)
    {
        if (desglose.Horas <= 0 || desglose.Monto <= 0 || desglose.PorcentajeCobro is <= 0 or > 100)
            throw new ArgumentException("Las horas, el monto y el porcentaje de cobro deben ser mayores a 0.");
    }

    public int Eliminar(int id)
    {
        return repositorio.Eliminar(id);
    }
}
