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

        // EXT2 devuelve solo las solicitudes publicadas que aun estan vigentes
        public List<SolicitudPublicadaDto> ListarSolicitudesPublicadas()
        {
            return _solicitudes.ListarPublicadas().ToList();
        }

        // EXT3 devuelve la informacion publica de una solicitud
        public SolicitudDetalleExternoDto? ObtenerDetalleSolicitud(int idSolicitud)
        {
            return _solicitudes.ObtenerDetalle(idSolicitud);
        }

        // EXT4 recibe y registra la participacion enviada desde wordpress
        public ResultadoOperacionDto RegistrarParticipacion(int idSolicitud, ParticipacionSolicitudDto participacion)
        {
            return _solicitudes.RegistrarParticipacion(idSolicitud, participacion);
        }

        // EXT4 revisa si hay que pedir los datos para crear un proveedor nuevo
        public ConsultaProveedorDto ConsultarProveedor(string cedulaJuridica)
        {
            return _solicitudes.ConsultarProveedor(cedulaJuridica);
        }

        // EXT6 consulta las participaciones anteriores de un proveedor
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
