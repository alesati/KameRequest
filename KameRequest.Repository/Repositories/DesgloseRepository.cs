using Dapper;
using KameRequest.Models;

namespace KameRequest.Repository.Repositories;

public class DesgloseRepository(IDbConnectionFactory conexiones)
{
    public IReadOnlyList<DesgloseSolicitud> ListarPorSolicitud(int idSolicitud)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT IdDesglose, IdSolicitud, Mes, Anio, Horas, Monto, PorcentajeCobro, Observaciones
            FROM DesglosesSolicitud
            WHERE IdSolicitud = @idSolicitud
            ORDER BY Anio, Mes, IdDesglose;
            """;
        return conexion.Query<DesgloseSolicitud>(sql, new { idSolicitud }).ToList();
    }

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
        return conexion.ExecuteScalar<int>("SELECT CAST(SCOPE_IDENTITY() AS int);");
    }

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

    public int Eliminar(int id)
    {
        using var conexion = conexiones.CreateConnection();
        return conexion.Execute("DELETE FROM DesglosesSolicitud WHERE IdDesglose = @id;", new { id });
    }
}

