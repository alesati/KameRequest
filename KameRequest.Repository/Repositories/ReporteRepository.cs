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
    public IReadOnlyList<Rpt_FacturacionMes> FacturacionPorMes(int mes, int anio, int pagina, int tamano)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT s.ConsecutivoOficio, s.Titulo, d.Mes, d.Anio, d.Monto,
                   d.Monto * 0.13 AS Iva, d.Monto * 1.13 AS Total
            FROM Solicitudes s
            INNER JOIN DesglosesSolicitud d ON d.IdSolicitud = s.IdSolicitud
            WHERE d.Mes = @mes AND d.Anio = @anio
            ORDER BY s.ConsecutivoOficio
            LIMIT @tamano OFFSET @offset;
            """;
        var offset = (pagina - 1) * tamano;
        return conexion.Query<Rpt_FacturacionMes>(sql, new { mes, anio, tamano, offset }).ToList();
    }

    public int ContarFacturacionPorMes(int mes, int anio)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT COUNT(*)
            FROM DesglosesSolicitud
            WHERE Mes = @mes AND Anio = @anio;
            """;
        return conexion.ExecuteScalar<int>(sql, new { mes, anio });
    }

    public Rpt_FacturacionMes TotalesFacturacionPorMes(int mes, int anio)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT COALESCE(SUM(Monto), 0) AS Monto,
                   COALESCE(SUM(Monto) * 0.13, 0) AS Iva,
                   COALESCE(SUM(Monto) * 1.13, 0) AS Total
            FROM DesglosesSolicitud
            WHERE Mes = @mes AND Anio = @anio;
            """;
        return conexion.QuerySingle<Rpt_FacturacionMes>(sql, new { mes, anio });
    }
}


