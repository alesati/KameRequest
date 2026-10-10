using Dapper;
using KameRequest.Models;
using MySqlConnector;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Claims;
namespace KameRequest.Clases;

    public class NotificacionesCorreos(IConfiguration configuracion,BitacoraRegistro bitacora, IWebHostEnvironment entorno)
    {
        public void EnviarCorreoCambioEstadoManual(ClaimsPrincipal usuario, string? destinatario,string estadoAnterior,string estadoNuevo,string consecutivo) {
        if (string.IsNullOrWhiteSpace(destinatario))
        {
            return;
        }

        try
		{
            var consechtml = WebUtility.HtmlEncode(consecutivo);
            var nombreUsuario = usuario.Identity?.Name ?? "Usuario desconocido";
            var usuariohtml = WebUtility.HtmlEncode(nombreUsuario);
            var anteriorhtml = WebUtility.HtmlEncode(estadoAnterior);
            var nuevohtml = WebUtility.HtmlEncode(estadoNuevo);
            var rutaLogo = Path.Combine(entorno.WebRootPath, "img", "logo-kamerequest.png");
            var logoHtml = File.Exists(rutaLogo)
                ? "<img src=\"cid:logoKamerequest\" alt=\"KameRequest\" style=\"width:110px;height:auto;display:block;margin:0 auto;\" />"
                : "<div style=\"font-size:24px;font-weight:700;color:#102D54;\">KameRequest</div>";

            var contenidoHtml = $"""
            <!DOCTYPE html>
            <html lang="es">
            <body style="margin:0;padding:24px;background:#F5F7FB;font-family:Arial,sans-serif;color:#334155;">
                <div style="max-width:600px;margin:0 auto;background:#FFFFFF;border:1px solid #DCE3EC;border-radius:12px;overflow:hidden;">
                    <div style="padding:24px;text-align:center;background:#FFFFFF;border-bottom:5px solid #FF8A24;">
                        {logoHtml}
                    </div>
                    <div style="padding:28px;">
                        <h1 style="margin:0 0 16px;color:#102D54;font-size:23px;">Cambio de estado de solicitud</h1>
                        <p style="margin:0 0 20px;line-height:1.5;">Se registró un cambio en la solicitud <strong>{consechtml}</strong>.</p>
                        <table role="presentation" style="width:100%;border-collapse:collapse;">
                            <tr><td style="padding:10px;border-bottom:1px solid #DCE3EC;font-weight:bold;">Estado anterior</td><td style="padding:10px;border-bottom:1px solid #DCE3EC;">{anteriorhtml}</td></tr>
                            <tr><td style="padding:10px;border-bottom:1px solid #DCE3EC;font-weight:bold;">Estado nuevo</td><td style="padding:10px;border-bottom:1px solid #DCE3EC;color:#10ADC0;font-weight:bold;">{nuevohtml}</td></tr>
                            <tr><td style="padding:10px;font-weight:bold;">Modificado por</td><td style="padding:10px;">{usuariohtml}</td></tr>
                        </table>
                    </div>
                    <div style="padding:16px 28px;background:#102D54;color:#FFFFFF;font-size:12px;text-align:center;">Notificación automática de KameRequest</div>
                </div>
            </body>
            </html>
            """;

            string asunto_correo = $"Cambio de estado de solicitud {consecutivo}";
            enviarCorreo(asunto_correo, contenidoHtml, destinatario,consecutivo);
            bitacora.Registrar(usuario, $"Envio de correo de cambio manual de estado de la solicitud #{consecutivo}");
        }
		catch (Exception ex)
		{
            bitacora.RegistrarErrorTecnicoSistema("envio de correo", ex);
            throw;
		}
		}

    public void EnviarCorreoCambioEstadoAuto(int idsolicitud)
    {
        try
        {
            using var conexion = CrearConexion();
            var solicitudenviar = conexion.QueryFirstOrDefault<dynamic>("""
                
                    SELECT
                    s.ConsecutivoOficio AS Consecutivo,
                    s.FechaIngreso,
                    r.Email AS Email
                    FROM Solicitudes s
                    LEFT JOIN Representantes r ON r.IdRepresentante = s.IdRepresentante
                    WHERE s.IdSolicitud = @id;
                
                """, new { id = idsolicitud });

            if (solicitudenviar is null)
            {
                return; // No se encontró la solicitud
            }

            string consecutivo = solicitudenviar.Consecutivo;
            string? destinatario = solicitudenviar.Email;
            if (string.IsNullOrWhiteSpace(destinatario))
            {
                return;
            }

            var consechtml = WebUtility.HtmlEncode(consecutivo);
            var anteriorhtml = WebUtility.HtmlEncode("Abierta");
            var nuevohtml = WebUtility.HtmlEncode("Vencida");
            var rutaLogo = Path.Combine(entorno.WebRootPath, "img", "logo-kamerequest.png");
            var logoHtml = File.Exists(rutaLogo)
                ? "<img src=\"cid:logoKamerequest\" alt=\"KameRequest\" style=\"width:110px;height:auto;display:block;margin:0 auto;\" />"
                : "<div style=\"font-size:24px;font-weight:700;color:#102D54;\">KameRequest</div>";

            var contenidoHtml = $"""
            <!DOCTYPE html>
            <html lang="es">
            <body style="margin:0;padding:24px;background:#F5F7FB;font-family:Arial,sans-serif;color:#334155;">
                <div style="max-width:600px;margin:0 auto;background:#FFFFFF;border:1px solid #DCE3EC;border-radius:12px;overflow:hidden;">
                    <div style="padding:24px;text-align:center;background:#FFFFFF;border-bottom:5px solid #FF8A24;">
                        {logoHtml}
                    </div>
                    <div style="padding:28px;">
                        <h1 style="margin:0 0 16px;color:#102D54;font-size:23px;">Cambio de estado de solicitud</h1>
                        <p style="margin:0 0 20px;line-height:1.5;">Se registró un cambio en la solicitud <strong>{consechtml}</strong>.</p>
                        <table role="presentation" style="width:100%;border-collapse:collapse;">
                            <tr><td style="padding:10px;border-bottom:1px solid #DCE3EC;font-weight:bold;">Estado anterior</td><td style="padding:10px;border-bottom:1px solid #DCE3EC;">{anteriorhtml}</td></tr>
                            <tr><td style="padding:10px;border-bottom:1px solid #DCE3EC;font-weight:bold;">Estado nuevo</td><td style="padding:10px;border-bottom:1px solid #DCE3EC;color:#10ADC0;font-weight:bold;">{nuevohtml}</td></tr>
                            <tr><td style="padding:10px;font-weight:bold;">Modificado por</td><td style="padding:10px;">Proceso Automatico</td></tr>
                        </table>
                    </div>
                    <div style="padding:16px 28px;background:#102D54;color:#FFFFFF;font-size:12px;text-align:center;">Notificación automática de KameRequest</div>
                </div>
            </body>
            </html>
            """;

            string asunto_correo = $"Cambio de estado de solicitud {consecutivo}";
            enviarCorreo(asunto_correo, contenidoHtml, destinatario, consecutivo);
            bitacora.RegistrarSistema($"Envio de correo de cambio automatico de estado de la solicitud #{consecutivo}");
        }
        catch (Exception ex)
        {
            bitacora.RegistrarErrorTecnicoSistema("envio de correo", ex);
            throw;
        }
    }

    private void enviarCorreo(string asunto, string cuerpo, string destinatario,string consecutivo)
    {
        try
        {
            var rutaLogo = Path.Combine(entorno.WebRootPath, "img", "logo-kamerequest.png");
            var logoHtml = File.Exists(rutaLogo)
                ? "<img src=\"cid:logoKamerequest\" alt=\"KameRequest\" style=\"width:110px;height:auto;display:block;margin:0 auto;\" />"
                : "<div style=\"font-size:24px;font-weight:700;color:#102D54;\">KameRequest</div>";


            using var mensaje = new MailMessage
            {
                From = new MailAddress(configuracion["Correo:Usuario"] ?? throw new InvalidOperationException("Falta configurar Correo:Usuario")),
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = true
            };

            mensaje.To.Add(destinatario);

            using var cliente = new SmtpClient(
            configuracion["Correo:Servidor"] ?? throw new InvalidOperationException("Falta configurar Correo:Servidor"),
            configuracion.GetValue<int>("Correo:Puerto"))
            {
                EnableSsl = configuracion.GetValue<bool>("Correo:UsarSsl"),
                Credentials = new NetworkCredential(
                configuracion["Correo:Usuario"],
                configuracion["Correo:Contrasena"])
            };
            // Se adjunta el logo dentro del mensaje para que el correo no necesite una URL publica.
            if (File.Exists(rutaLogo))
            {
                var vistaHtml = AlternateView.CreateAlternateViewFromString(cuerpo, null, MediaTypeNames.Text.Html);
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
            throw ;
        }
    }
    private MySqlConnection CrearConexion() =>
        new(configuracion.GetConnectionString("MySql"));


}







    



