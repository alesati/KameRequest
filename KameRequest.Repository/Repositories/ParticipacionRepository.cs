using Dapper;
using KameRequest.Models;

namespace KameRequest.Repository.Repositories;

// Consultas internas para revisar las propuestas enviadas desde WordPress.
public class ParticipacionRepository(IDbConnectionFactory conexiones)
{
    public IReadOnlyList<ParticipacionSolicitud> ListarPorSolicitud(int idSolicitud)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT p.IdParticipacion, p.IdSolicitud, p.IdProveedor, p.FechaParticipacion, p.Estado,
                   pr.CedulaJuridica, pr.NombreProveedor, pr.NombreRepresentante, pr.Telefono, pr.Correo
            FROM ParticipacionesSolicitud p
            INNER JOIN Proveedores pr ON pr.IdProveedor = p.IdProveedor
            WHERE p.IdSolicitud = @idSolicitud
            ORDER BY p.FechaParticipacion DESC;
            """;
        return conexion.Query<ParticipacionSolicitud>(sql, new { idSolicitud }).ToList();
    }

    // revisa si la solicitud ya tiene una propuesta aprobada
    public bool TieneAprobada(int idSolicitud)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT COUNT(*)
            FROM ParticipacionesSolicitud
            WHERE IdSolicitud = @idSolicitud
              AND Estado = 'Aprobada';
            """;
        return conexion.ExecuteScalar<int>(sql, new { idSolicitud }) > 0;
    }

    public IReadOnlyList<DesgloseParticipacion> ListarDesgloses(int idParticipacion)
    {
        using var conexion = conexiones.CreateConnection();
        const string sql = """
            SELECT IdDesgloseParticipacion, IdParticipacion, Mes, Anio, Horas, Monto,
                   PorcentajeCobro, Observaciones
            FROM DesglosesParticipacion
            WHERE IdParticipacion = @idParticipacion
            ORDER BY Anio, Mes;
            """;
        return conexion.Query<DesgloseParticipacion>(sql, new { idParticipacion }).ToList();
    }

    // Solo se puede aprobar una propuesta por solicitud.
    public bool Aprobar(int idSolicitud, int idParticipacion)
    {
        using var conexion = conexiones.CreateConnection();
        conexion.Open();
        using var transaccion = conexion.BeginTransaction();

        try
        {
            var existe = conexion.ExecuteScalar<int>("""
                SELECT COUNT(*) FROM ParticipacionesSolicitud
                WHERE IdParticipacion = @idParticipacion AND IdSolicitud = @idSolicitud;
                """, new { idSolicitud, idParticipacion }, transaccion) == 1;
            if (!existe)
            {
                transaccion.Rollback();
                return false;
            }

            var yaAprobada = conexion.ExecuteScalar<int>("""
                SELECT COUNT(*) FROM ParticipacionesSolicitud
                WHERE IdSolicitud = @idSolicitud AND Estado = 'Aprobada';
                """, new { idSolicitud }, transaccion) > 0;
            if (yaAprobada)
            {
                transaccion.Rollback();
                return false;
            }

            var tieneTareasRelacionadas = conexion.ExecuteScalar<int>("""
                SELECT COUNT(*)
                FROM Tareas t
                INNER JOIN DesglosesSolicitud d ON d.IdDesglose = t.IdDesglose
                WHERE d.IdSolicitud = @idSolicitud;
                """, new { idSolicitud }, transaccion) > 0;
            if (tieneTareasRelacionadas)
                throw new InvalidOperationException("No se puede aprobar porque existen tareas relacionadas con el desglose actual.");

            // La propuesta aprobada reemplaza el plan de cobro oficial de la solicitud.
            conexion.Execute("DELETE FROM DesglosesSolicitud WHERE IdSolicitud = @idSolicitud;", new { idSolicitud }, transaccion);
            conexion.Execute("""
                INSERT INTO DesglosesSolicitud
                    (IdSolicitud, Mes, Anio, Horas, Monto, PorcentajeCobro, Observaciones)
                SELECT @idSolicitud, Mes, Anio, Horas, Monto, PorcentajeCobro, Observaciones
                FROM DesglosesParticipacion
                WHERE IdParticipacion = @idParticipacion;
                """, new { idSolicitud, idParticipacion }, transaccion);

            conexion.Execute("UPDATE ParticipacionesSolicitud SET Estado = 'Aprobada' WHERE IdParticipacion = @idParticipacion;", new { idParticipacion }, transaccion);
            // al elegir una propuesta se retira la solicitud del portal externo
            conexion.Execute("""
                UPDATE Solicitudes
                SET IdParticipacionAprobada = @idParticipacion,
                    PublicadaExterna = 0,
                    FechaVencimientoPublicacion = NULL
                WHERE IdSolicitud = @idSolicitud;
                """, new { idSolicitud, idParticipacion }, transaccion);
            transaccion.Commit();
            return true;
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }
}