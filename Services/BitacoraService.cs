using System.Security.Claims;
using Dapper;
using KameRequest.Models;
using MySqlConnector;
using System.Text.Json;

namespace KameRequest.Services;

// Este servicio guarda las acciones importantes y despues permite consultarlas.
public class BitacoraService(IConfiguration configuracion)
{
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        WriteIndented = false
    };

    // Guarda en bitacora cuando se crea un registro, junto con los datos nuevos.
    public void RegistrarCreacion(ClaimsPrincipal usuario, string elemento, object nuevo)
    {
        Registrar(usuario, $"Creó {elemento}. Nuevo: {JsonSerializer.Serialize(nuevo, OpcionesJson)}");
    }

    // Guarda los datos antes y despues cuando se modifica un registro.
    public void RegistrarActualizacion(
        ClaimsPrincipal usuario,
        string elemento,
        object? anterior,
        object actual)
    {
        Registrar(usuario,
            $"Actualizó {elemento}. Anterior: {JsonSerializer.Serialize(anterior, OpcionesJson)}. " +
            $"Actual: {JsonSerializer.Serialize(actual, OpcionesJson)}");
    }

    // Guarda los datos antes de eliminar un registro.
    public void RegistrarEliminacion(ClaimsPrincipal usuario, string elemento, object? eliminado)
    {
        Registrar(usuario, $"Eliminó {elemento}. Datos: {JsonSerializer.Serialize(eliminado, OpcionesJson)}");
    }

    // Guarda un error tecnico que no se esperaba y que no se muestra al usuario final.
    public void RegistrarErrorTecnico(ClaimsPrincipal usuario, string ruta, Exception excepcion)
    {
        Registrar(usuario, $"ERROR TÉCNICO. Ruta: {ruta}. Mensaje: {excepcion.Message}");
    }

    // Guarda los errores que pasan en procesos automaticos del sistema.
    public void RegistrarErrorTecnicoSistema(string origen, Exception excepcion)
    {
        RegistrarSistema($"ERROR TÉCNICO. Origen: {origen}. Mensaje: {excepcion.Message}");
    }

    // Guarda una accion usando el usuario que esta en la sesion.
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

    // Guarda una accion cuando ya se conoce el id del usuario.
    public void Registrar(int idUsuario, string descripcion)
    {
        using var conexion = CrearConexion();
        conexion.Execute("""
            INSERT INTO Bitacoras (Fecha, IdUsuario, Descripcion)
            VALUES (NOW(), @idUsuario, @descripcion);
            """, new { idUsuario, descripcion });
    }

    // Guarda una accion automatica sin asignarla a una persona.
    public void RegistrarSistema(string descripcion)
    {
        using var conexion = CrearConexion();
        conexion.Execute("""
            INSERT INTO Bitacoras (Fecha, IdUsuario, Descripcion)
            VALUES (NOW(), NULL, @descripcion);
            """, new { descripcion });
    }

    // Cuenta los registros para saber cuantas paginas se necesitan.
    public int ContarRegistros()
    {
        using var conexion = CrearConexion();
        return conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Bitacoras");
    }

    // Trae una pagina de registros empezando por el mas reciente.
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

    // Devuelve una conexion para guardar o consultar la bitacora.
    private MySqlConnection CrearConexion() =>
        new(configuracion.GetConnectionString("MySql"));
}
