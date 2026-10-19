using System.Security.Claims;

namespace KameRequest.Services.Abstract;

// mantiene separado el envio de correos de las pantallas Razor
public interface INotificacionesCorreoService
{
    void EnviarCorreoCambioEstadoManual(ClaimsPrincipal usuario, string? destinatario, string estadoAnterior, string estadoNuevo, string consecutivo);
    void EnviarCorreoCambioEstadoAuto(int idSolicitud);
}
