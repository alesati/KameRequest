using System.Security.Claims;
using System.Text.Json;
using KameRequest.Models;
using KameRequest.Repository.Repositories;

namespace KameRequest.Clases;

// Forma los mensajes de auditoria; BitacoraRepository es quien guarda en SQL Server.
// prepara los datos que se guardan en la bitacora
public class BitacoraRegistro(BitacoraRepository repositorio)
{
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        WriteIndented = false
    };

    // guarda en bitacora los datos del registro creado
    public void RegistrarCreacion(ClaimsPrincipal usuario, string elemento, object nuevo)
    {
        var descripcion = $"Creo {elemento}. Nuevo: {JsonSerializer.Serialize(nuevo, OpcionesJson)}";

        Registrar(usuario, descripcion);
    }

    // guarda en bitacora los datos antes y despues del cambio
    public void RegistrarActualizacion(
        ClaimsPrincipal usuario,
        string elemento,
        object? anterior,
        object? actual)
    {
        var descripcion = $"Actualizo {elemento}. " +
                          $"Anterior: {JsonSerializer.Serialize(anterior, OpcionesJson)}. " +
                          $"Actual: {JsonSerializer.Serialize(actual, OpcionesJson)}";

        Registrar(usuario, descripcion);
    }

    // guarda en bitacora los datos del registro eliminado
    public void RegistrarEliminacion(ClaimsPrincipal usuario, string elemento, object? eliminado)
    {
        var descripcion = $"Elimino {elemento}. " +
                          $"Datos: {JsonSerializer.Serialize(eliminado, OpcionesJson)}";

        Registrar(usuario, descripcion);
    }

    // deja evidencia de una consulta importante
    public void RegistrarConsulta(ClaimsPrincipal usuario, string elemento)
    {
        Registrar(usuario, $"El usuario consulta {elemento}");
    }

    // guarda el error tecnico sin mostrar detalles internos al usuario
    public void RegistrarErrorTecnico(ClaimsPrincipal usuario, string ruta, Exception excepcion)
    {
        var descripcion = $"ERROR TECNICO. Ruta: {ruta}. Mensaje: {excepcion.Message}";

        Registrar(usuario, descripcion);
    }

    // registra errores producidos por procesos automaticos
    public void RegistrarErrorTecnicoSistema(string origen, Exception excepcion)
    {
        var descripcion = $"ERROR TECNICO. Origen: {origen}. Mensaje: {excepcion.Message}";

        repositorio.Registrar(null, descripcion);
    }

    public void Registrar(ClaimsPrincipal usuarioActual, string descripcion)
    {
        var texto = usuarioActual.FindFirstValue(ClaimTypes.NameIdentifier);
        int? idUsuario = int.TryParse(texto, out var id) ? id : null;

        repositorio.Registrar(idUsuario, descripcion);
    }

    public void Registrar(int idUsuario, string descripcion)
    {
        repositorio.Registrar(idUsuario, descripcion);
    }

    public void RegistrarSistema(string descripcion)
    {
        repositorio.Registrar(null, descripcion);
    }

    public int ContarRegistros()
    {
        return repositorio.Contar();
    }

    // obtiene solo los registros de la pagina solicitada
    public IReadOnlyList<Bitacora> ObtenerPagina(int pagina, int registrosPorPagina)
    {
        return repositorio.ObtenerPagina(pagina, registrosPorPagina);
    }
}



