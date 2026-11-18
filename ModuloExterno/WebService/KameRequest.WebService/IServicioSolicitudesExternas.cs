using System.Collections.Generic;
using CoreWCF;
using KameRequest.WebService.Modelos;

namespace KameRequest.WebService
{
    // contrato que define las operaciones publicas del web service WCF
    [ServiceContract]
    public interface IServicioSolicitudesExternas
    {
        
        [OperationContract]
        List<SolicitudPublicadaDto> ListarSolicitudesPublicadas();

        [OperationContract]
        SolicitudDetalleExternoDto? ObtenerDetalleSolicitud(int idSolicitud);

        [OperationContract]
        ResultadoOperacionDto RegistrarParticipacion(int idSolicitud, ParticipacionSolicitudDto participacion);
       
        [OperationContract]
        ConsultaProveedorDto ConsultarProveedor(string cedulaJuridica);

        [OperationContract]
        ConsultaParticipacionesDto ConsultarParticipacionesProveedor(string cedulaJuridica);
    }
}
