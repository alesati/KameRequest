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

    public IReadOnlyList<Rpt_FacturacionMes> FacturacionPorMes(int mes, int anio, int pagina, int tamano)
    {
        return repositorio.FacturacionPorMes(mes, anio, pagina, tamano);
    }

    // cuenta las filas para calcular cuantas paginas mostrar
    public int ContarFacturacionPorMes(int mes, int anio)
    {
        return repositorio.ContarFacturacionPorMes(mes, anio);
    }

    // conserva los totales de todo el periodo aunque la tabla este paginada
    public Rpt_FacturacionMes TotalesFacturacionPorMes(int mes, int anio)
    {
        return repositorio.TotalesFacturacionPorMes(mes, anio);
    }
}
