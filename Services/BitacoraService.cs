using System.Security.Claims;
using System.Text.Json;
using Dapper;
using KameRequest.Models;
using MySqlConnector;

namespace KameRequest.Services;

// Guarda y consulta acciones importantes realizadas en el sistema.
public class BitacoraService(IConfiguration configuracion)
{
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        WriteIndented = false
    };

    public void RegistrarCreacion(ClaimsPrincipal usuario, string elemento, object nuevo)
    {
        Registrar(usuario, $"Creo {elemento}. Nuevo: {JsonSerializer.Serialize(nuevo, OpcionesJson)}");
    }

    public void RegistrarActualizacion(ClaimsPrincipal usuario, string elemento, object? anterior, object? actual)
    {
        Registrar(usuario,
            $"Actualizo {elemento}. Anterior: {JsonSerializer.Serialize(anterior, OpcionesJson)}. " +
            $"Actual: {JsonSerializer.Serialize(actual, OpcionesJson)}");
    }

    public void RegistrarEliminacion(ClaimsPrincipal usuario, string elemento, object? eliminado)
    {
        Registrar(usuario, $"Elimino {elemento}. Datos: {JsonSerializer.Serialize(eliminado, OpcionesJson)}");
    }

    // Las consultas deben registrar exclusivamente el texto definido por el enunciado.
    public void RegistrarConsulta(ClaimsPrincipal usuario, string elemento)
    {
        Registrar(usuario, $"El usuario consulta {elemento}");
    }

    public void RegistrarErrorTecnico(ClaimsPrincipal usuario, string ruta, Exception excepcion)
    {
        Registrar(usuario, $"ERROR TECNICO. Ruta: {ruta}. Mensaje: {excepcion.Message}");
    }

    public void RegistrarErrorTecnicoSistema(string origen, Exception excepcion)
    {
        RegistrarSistema($"ERROR TECNICO. Origen: {origen}. Mensaje: {excepcion.Message}");
    }

    public void Registrar(ClaimsPrincipal usuarioActual, string descripcion)
    {
        var textoId = usuarioActual.FindFirstValue(ClaimTypes.NameIdentifier);
        int? idUsuario = int.TryParse(textoId, out var id) ? id : null;

        using var conexion = CrearConexion();
        conexion.Execute("""
            INSERT INTO Bitacoras (Fecha, IdUsuario, Descripcion)
            VALUES (NOW(), @idUsuario, @descripcion);
            """, new { idUsuario, descripcion });
    }

    public void Registrar(int idUsuario, string descripcion)
    {
        using var conexion = CrearConexion();
        conexion.Execute("""
            INSERT INTO Bitacoras (Fecha, IdUsuario, Descripcion)
            VALUES (NOW(), @idUsuario, @descripcion);
            """, new { idUsuario, descripcion });
    }

    public void RegistrarSistema(string descripcion)
    {
        using var conexion = CrearConexion();
        conexion.Execute("""
            INSERT INTO Bitacoras (Fecha, IdUsuario, Descripcion)
            VALUES (NOW(), NULL, @descripcion);
            """, new { descripcion });
    }

    public int ContarRegistros()
    {
        using var conexion = CrearConexion();
        return conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Bitacoras");
    }

    public IReadOnlyList<Bitacora> ObtenerPagina(int pagina, int registrosPorPagina)
    {
        using var conexion = CrearConexion();
        return conexion.Query<Bitacora>("""
            SELECT b.Fecha, COALESCE(u.NombreCompleto, 'Sistema') AS Usuario, b.Descripcion
            FROM Bitacoras b
            LEFT JOIN Usuarios u ON u.IdUsuario = b.IdUsuario
            ORDER BY b.Fecha DESC
            LIMIT @registrosPorPagina OFFSET @offset;
            """, new
        {
            registrosPorPagina,
            offset = (pagina - 1) * registrosPorPagina
        }).ToList();
    }

    private MySqlConnection CrearConexion() =>
        new(configuracion.GetConnectionString("MySql"));
}
