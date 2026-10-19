using KameRequest.Models;
using KameRequest.Repository.Repositories;
using KameRequest.Services.Abstract;

namespace KameRequest.Services;

// entrega los datos de los reportes sin exponer consultas SQL a las pantallas
public class ReporteService(ReporteRepository repositorio) : IReporteService
{
    public IReadOnlyList<string> ListarUsuariosConTareas()
    {
        return repositorio.ListarUsuariosConTareas();
    }

    public IReadOnlyList<TareaReporteDto> TareasPorUsuario(string usuario, int mes, int anio)
    {
        return repositorio.TareasPorUsuario(usuario, mes, anio);
    }

    public IReadOnlyList<TareaReporteDto> TareasPorSolicitud(int idSolicitud, int mes, int anio)
    {
        return repositorio.TareasPorSolicitud(idSolicitud, mes, anio);
    }

    public IReadOnlyList<Rpt_FacturacionMes> FacturacionPorMes(int mes, int anio)
    {
        return repositorio.FacturacionPorMes(mes, anio);
    }
}
