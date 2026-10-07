using Dapper;
using KameRequest.Models;

namespace KameRequest.Repository.Repositories;

public class ReporteRepository(IDbConnectionFactory conexiones)
{
    public IReadOnlyList<string> ListarUsuariosConTareas()
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT DISTINCT u.NombreCompleto
            FROM Tareas t
            INNER JOIN Usuarios u ON u.IdUsuario = t.IdUsuario
            ORDER BY u.NombreCompleto;
            """;
        return conexion.Query<string>(sql).ToList();
    }

    public IReadOnlyList<TareaReporteDto> TareasPorUsuario(string usuario, int mes, int anio)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT u.NombreCompleto AS Usuario, t.Fecha, t.Descripcion, t.HorasInvertidas,
                   s.ConsecutivoOficio, s.Titulo AS TituloSolicitud
            FROM Tareas t
            INNER JOIN Usuarios u ON u.IdUsuario = t.IdUsuario
            INNER JOIN Solicitudes s ON t.IdSolicitud = s.IdSolicitud
            WHERE u.NombreCompleto = @usuario
              AND MONTH(t.Fecha) = @mes
              AND YEAR(t.Fecha) = @anio
            ORDER BY t.Fecha ASC;
            """;
        return conexion.Query<TareaReporteDto>(sql, new { usuario, mes, anio }).ToList();
    }

    public IReadOnlyList<TareaReporteDto> TareasPorSolicitud(int idSolicitud, int mes, int anio)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT u.NombreCompleto AS Usuario, t.Fecha, t.Descripcion, t.HorasInvertidas,
                   s.ConsecutivoOficio, s.Titulo AS TituloSolicitud
            FROM Tareas t
            INNER JOIN Usuarios u ON u.IdUsuario = t.IdUsuario
            INNER JOIN Solicitudes s ON t.IdSolicitud = s.IdSolicitud
            WHERE t.IdSolicitud = @idSolicitud
              AND MONTH(t.Fecha) = @mes
              AND YEAR(t.Fecha) = @anio
            ORDER BY t.Fecha ASC;
            """;
        return conexion.Query<TareaReporteDto>(sql, new { idSolicitud, mes, anio }).ToList();
    }
}
