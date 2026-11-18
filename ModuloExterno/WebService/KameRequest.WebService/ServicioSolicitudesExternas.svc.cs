using System.Collections.Generic;
using System.Linq;
using KameRequest.WebService.LogicaNegocio.Contratos;
using KameRequest.WebService.Modelos;

namespace KameRequest.WebService
{
    // implementa el contrato WCF y dirige cada operacion a la logica de negocio
    public class ServicioSolicitudesExternas : IServicioSolicitudesExternas
    {
        private readonly ISolicitudesExternasServicio _solicitudes;

        public ServicioSolicitudesExternas(ISolicitudesExternasServicio solicitudes)
        {
            _solicitudes = solicitudes;
        }

        public List<SolicitudPublicadaDto> ListarSolicitudesPublicadas()
        {
            return _solicitudes.ListarPublicadas().ToList();
        }

        public SolicitudDetalleExternoDto? ObtenerDetalleSolicitud(int idSolicitud)
        {
            return _solicitudes.ObtenerDetalle(idSolicitud);
        }

        public ResultadoOperacionDto RegistrarParticipacion(int idSolicitud, ParticipacionSolicitudDto participacion)
        {
            return _solicitudes.RegistrarParticipacion(idSolicitud, participacion);
        }

        public ConsultaProveedorDto ConsultarProveedor(string cedulaJuridica)
        {
            return _solicitudes.ConsultarProveedor(cedulaJuridica);
        }
        public ConsultaParticipacionesDto ConsultarParticipacionesProveedor(string cedulaJuridica)
        {
            IEnumerable<ParticipacionProveedorDto> participaciones;
            var resultado = _solicitudes.ConsultarParticipaciones(cedulaJuridica, out participaciones);

            return new ConsultaParticipacionesDto
            {
                Resultado = resultado,
                Participaciones = participaciones.ToList()
            };
        }
    }
}
