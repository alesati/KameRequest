using Dapper;
using KameRequest.Models;

namespace KameRequest.Repository.Repositories;

public class DesgloseRepository(IDbConnectionFactory conexiones)
{
    // obtiene los registros relacionados con una solicitud y su pagina
    public IReadOnlyList<DesgloseSolicitud> ObtenerPaginaPorSolicitud(int idSolicitud, int pagina, int tamano)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT IdDesglose, IdSolicitud, Mes, Anio, Horas, Monto, PorcentajeCobro, Observaciones
            FROM DesglosesSolicitud
            WHERE IdSolicitud = @idSolicitud
            ORDER BY Anio, Mes, IdDesglose
            LIMIT @tamano OFFSET @offset;
            """;
        return conexion.Query<DesgloseSolicitud>(sql, new { idSolicitud, tamano, offset = (pagina - 1) * tamano }).ToList();
    }

    // cuenta los registros que pertenecen a la solicitud
    public int ContarPorSolicitud(int idSolicitud)
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM DesglosesSolicitud WHERE IdSolicitud = @idSolicitud;", new { idSolicitud });
    }

    public decimal SumarPorcentajePorSolicitud(int idSolicitud, int? excluirIdDesglose = null)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT COALESCE(SUM(PorcentajeCobro), 0)
            FROM DesglosesSolicitud
            WHERE IdSolicitud = @idSolicitud
              AND (@excluirIdDesglose IS NULL OR IdDesglose <> @excluirIdDesglose);
            """;
        return conexion.ExecuteScalar<decimal>(sql, new { idSolicitud, excluirIdDesglose });
    }
    // busca un registro por su identificador
    public DesgloseSolicitud? Obtener(int id)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT IdDesglose, IdSolicitud, Mes, Anio, Horas, Monto, PorcentajeCobro, Observaciones
            FROM DesglosesSolicitud
            WHERE IdDesglose = @id;
            """;
        return conexion.QuerySingleOrDefault<DesgloseSolicitud>(sql, new { id });
    }

    // guarda un nuevo registro en la base de datos
    public int Crear(DesgloseSolicitud desglose)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            INSERT INTO DesglosesSolicitud
                (IdSolicitud, Mes, Anio, Horas, Monto, PorcentajeCobro, Observaciones)
            VALUES
                (@IdSolicitud, @Mes, @Anio, @Horas, @Monto, @PorcentajeCobro, @Observaciones);
            """;
        conexion.Execute(sql, desglose);
        return conexion.ExecuteScalar<int>("SELECT LAST_INSERT_ID();");
    }

    // guarda los cambios realizados al registro
    public void Actualizar(DesgloseSolicitud desglose)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            UPDATE DesglosesSolicitud
            SET Mes = @Mes,
                Anio = @Anio,
                Horas = @Horas,
                Monto = @Monto,
                PorcentajeCobro = @PorcentajeCobro,
                Observaciones = @Observaciones
            WHERE IdDesglose = @IdDesglose;
            """;
        conexion.Execute(sql, desglose);
    }

    // elimina el registro indicado si no tiene relaciones
    public int Eliminar(int id)
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.Execute("DELETE FROM DesglosesSolicitud WHERE IdDesglose = @id;", new { id });
    }
}
