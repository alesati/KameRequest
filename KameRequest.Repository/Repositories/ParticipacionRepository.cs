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

            conexion.Execute("UPDATE ParticipacionesSolicitud SET Estado = 'Aprobada' WHERE IdParticipacion = @idParticipacion;", new { idParticipacion }, transaccion);
            conexion.Execute("UPDATE Solicitudes SET IdParticipacionAprobada = @idParticipacion WHERE IdSolicitud = @idSolicitud;", new { idSolicitud, idParticipacion }, transaccion);
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