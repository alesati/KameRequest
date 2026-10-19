using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Claims;
using KameRequest.Services.Abstract;

namespace KameRequest.Services;

// prepara los correos de cambio de estado y los envia por SMTP
public class NotificacionesCorreoService(
    IConfiguration configuracion,
    IBitacoraService bitacora,
    IWebHostEnvironment entorno,
    ISolicitudService solicitudes) : INotificacionesCorreoService
{
    // se usa cuando una persona cambia el estado desde la pantalla
    public void EnviarCorreoCambioEstadoManual(
        ClaimsPrincipal usuario,
        string? destinatario,
        string estadoAnterior,
        string estadoNuevo,
        string consecutivo)
    {
        // sin correo no hay a quien enviar la notificacion
        if (string.IsNullOrWhiteSpace(destinatario))
        {
            return;
        }

        try
        {
            var nombreUsuario = usuario.Identity?.Name ?? "Usuario desconocido";
            var contenidoHtml = CrearContenidoHtml(
                consecutivo,
                estadoAnterior,
                estadoNuevo,
                nombreUsuario);

            EnviarCorreo(
                $"Cambio de estado de solicitud {consecutivo}",
                contenidoHtml,
                destinatario);

            bitacora.Registrar(
                usuario,
                $"Envio de correo de cambio manual de estado de la solicitud #{consecutivo}");
        }
        catch (Exception ex)
        {
            bitacora.RegistrarErrorTecnicoSistema("envio de correo", ex);
            throw;
        }
    }

    // se usa cuando el proceso automatico marca una solicitud como vencida
    public void EnviarCorreoCambioEstadoAuto(int idSolicitud)
    {
        try
        {
            // busca los datos completos de la solicitud antes de enviar el correo
            var solicitud = solicitudes.Obtener(idSolicitud);
            if (solicitud is null || string.IsNullOrWhiteSpace(solicitud.CorreoRepresentante))
            {
                return;
            }

            var contenidoHtml = CrearContenidoHtml(
                solicitud.ConsecutivoOficio,
                "Abierta",
                "Vencida",
                "Sistema");

            EnviarCorreo(
                $"Cambio de estado de solicitud {solicitud.ConsecutivoOficio}",
                contenidoHtml,
                solicitud.CorreoRepresentante);

            bitacora.RegistrarSistema(
                $"Envio de correo de cambio automatico de estado de la solicitud #{solicitud.ConsecutivoOficio}");
        }
        catch (Exception ex)
        {
            bitacora.RegistrarErrorTecnicoSistema("envio de correo", ex);
            throw;
        }
    }

    // crea el contenido HTML que se reutiliza para ambos tipos de aviso
    private string CrearContenidoHtml(
        string consecutivo,
        string estadoAnterior,
        string estadoNuevo,
        string modificadoPor)
    {
        // codifica datos que vienen de la base para mostrarlos como texto seguro
        var consecutivoHtml = WebUtility.HtmlEncode(consecutivo);
        var anteriorHtml = WebUtility.HtmlEncode(estadoAnterior);
        var nuevoHtml = WebUtility.HtmlEncode(estadoNuevo);
        var usuarioHtml = WebUtility.HtmlEncode(modificadoPor);
        var rutaLogo = ObtenerRutaLogo();

        // si no existe la imagen, se conserva el nombre del sistema como encabezado
        var logoHtml = File.Exists(rutaLogo)
            ? "<img src=\"cid:logoKamerequest\" alt=\"KameRequest\" style=\"width:110px;height:auto;display:block;margin:0 auto;\" />"
            : "<div style=\"font-size:24px;font-weight:700;color:#102D54;\">KameRequest</div>";

        return $"""
            <!DOCTYPE html>
            <html lang="es">
            <body style="margin:0;padding:24px;background:#F5F7FB;font-family:Arial,sans-serif;color:#334155;">
                <div style="max-width:600px;margin:0 auto;background:#FFFFFF;border:1px solid #DCE3EC;border-radius:12px;overflow:hidden;">
                    <div style="padding:24px;text-align:center;background:#FFFFFF;border-bottom:5px solid #FF8A24;">
                        {logoHtml}
                    </div>
                    <div style="padding:28px;">
                        <h1 style="margin:0 0 16px;color:#102D54;font-size:23px;">Cambio de estado de solicitud</h1>
                        <p style="margin:0 0 20px;line-height:1.5;">Se registró un cambio en la solicitud <strong>{consecutivoHtml}</strong>.</p>
                        <table role="presentation" style="width:100%;border-collapse:collapse;">
                            <tr>
                                <td style="padding:10px;border-bottom:1px solid #DCE3EC;font-weight:bold;">Estado anterior</td>
                                <td style="padding:10px;border-bottom:1px solid #DCE3EC;">{anteriorHtml}</td>
                            </tr>
                            <tr>
                                <td style="padding:10px;border-bottom:1px solid #DCE3EC;font-weight:bold;">Estado nuevo</td>
                                <td style="padding:10px;border-bottom:1px solid #DCE3EC;color:#10ADC0;font-weight:bold;">{nuevoHtml}</td>
                            </tr>
                            <tr><td style="padding:10px;font-weight:bold;">Modificado por</td><td style="padding:10px;">{usuarioHtml}</td></tr>
                        </table>
                    </div>
                    <div style="padding:16px 28px;background:#102D54;color:#FFFFFF;font-size:12px;text-align:center;">Notificación automática de KameRequest</div>
                </div>
            </body>
            </html>
            """;
    }

    // hace la conexion SMTP y adjunta el logo dentro del correo
    private void EnviarCorreo(string asunto, string cuerpo, string destinatario)
    {
        try
        {
            var correoOrigen = configuracion["Correo:Usuario"]
                ?? throw new InvalidOperationException("Falta configurar Correo:Usuario");
            var dominioCorreo = correoOrigen.Split('@').LastOrDefault() ?? "kamerequest.local";

            using var mensaje = new MailMessage
            {
                From = new MailAddress(
                    correoOrigen,
                    configuracion["Correo:NombreRemitente"] ?? "KameRequest"),
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = true
            };

            // Gmail solicita un identificador unico para aceptar el correo
            mensaje.Headers.Add("Message-ID", $"<{Guid.NewGuid():N}@{dominioCorreo}>");
            mensaje.To.Add(destinatario);

            using var cliente = new SmtpClient(
                configuracion["Correo:Servidor"]
                    ?? throw new InvalidOperationException("Falta configurar Correo:Servidor"),
                configuracion.GetValue<int>("Correo:Puerto"))
            {
                EnableSsl = configuracion.GetValue<bool>("Correo:UsarSsl"),
                Credentials = new NetworkCredential(
                    configuracion["Correo:Usuario"],
                    configuracion["Correo:Contrasena"])
            };

            var rutaLogo = ObtenerRutaLogo();
            if (File.Exists(rutaLogo))
            {
                var vistaHtml = AlternateView.CreateAlternateViewFromString(
                    cuerpo,
                    null,
                    MediaTypeNames.Text.Html);
                var logo = new LinkedResource(rutaLogo, MediaTypeNames.Image.Png)
                {
                    ContentId = "logoKamerequest",
                    TransferEncoding = TransferEncoding.Base64
                };

                vistaHtml.LinkedResources.Add(logo);
                mensaje.AlternateViews.Add(vistaHtml);
            }

            cliente.Send(mensaje);
        }
        catch (Exception ex)
        {
            bitacora.RegistrarErrorTecnicoSistema("envio de correo", ex);
            throw;
        }
    }

    // devuelve la ubicacion del logo dentro de wwwroot
    private string ObtenerRutaLogo()
    {
        return Path.Combine(entorno.WebRootPath, "img", "logo-kamerequest.png");
    }
}
