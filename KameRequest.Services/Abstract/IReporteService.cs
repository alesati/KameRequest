using KameRequest.Models;

namespace KameRequest.Services.Abstract;

// contrato para generar los datos de los reportes
public interface IReporteService
{
    IReadOnlyList<string> ListarUsuariosConTareas();
    IReadOnlyList<TareaReporteDto> TareasPorUsuario(string usuario, int mes, int anio);
    IReadOnlyList<TareaReporteDto> TareasPorSolicitud(int idSolicitud, int mes, int anio);
    IReadOnlyList<Rpt_FacturacionMes> FacturacionPorMes(int mes, int anio);
}
