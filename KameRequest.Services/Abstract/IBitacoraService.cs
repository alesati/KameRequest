using System.Security.Claims;
using KameRequest.Models;

namespace KameRequest.Services.Abstract;

// contrato que usa el sistema para guardar las acciones importantes
public interface IBitacoraService
{
    void RegistrarCreacion(ClaimsPrincipal usuario, string elemento, object nuevo);
    void RegistrarActualizacion(ClaimsPrincipal usuario, string elemento, object? anterior, object? actual);
    void RegistrarEliminacion(ClaimsPrincipal usuario, string elemento, object? eliminado);
    void RegistrarConsulta(ClaimsPrincipal usuario, string elemento);
    void RegistrarErrorTecnico(ClaimsPrincipal usuario, string ruta, Exception excepcion);
    void RegistrarErrorTecnicoSistema(string origen, Exception excepcion);
    void Registrar(ClaimsPrincipal usuarioActual, string descripcion);
    void Registrar(int idUsuario, string descripcion);
    void RegistrarSistema(string descripcion);
    int ContarRegistros();
    IReadOnlyList<Bitacora> ObtenerPagina(int pagina, int registrosPorPagina);
}
