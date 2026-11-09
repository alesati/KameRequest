using System.Collections.Generic;
using CoreWCF;
using KameRequest.WebService.Modelos;

namespace KameRequest.WebService
{
    // contrato que define las operaciones publicas del web service WCF
    [ServiceContract]
    public interface IServicioSolicitudesExternas
    {
        // EXT2 entrega el listado que se mostrara en wordpress
        [OperationContract]
        List<SolicitudPublicadaDto> ListarSolicitudesPublicadas();

        // EXT3 entrega la informacion de un oficio seleccionado
        [OperationContract]
        SolicitudDetalleExternoDto? ObtenerDetalleSolicitud(int idSolicitud);

        // EXT4 recibe la propuesta enviada por un proveedor
        [OperationContract]
        ResultadoOperacionDto RegistrarParticipacion(int idSolicitud, ParticipacionSolicitudDto participacion);

        // EXT4 indica si se deben solicitar datos para crear un proveedor
        [OperationContract]
        ConsultaProveedorDto ConsultarProveedor(string cedulaJuridica);

        // EXT6 devuelve el historial de participaciones de un proveedor
        [OperationContract]
        ConsultaParticipacionesDto ConsultarParticipacionesProveedor(string cedulaJuridica);
    }
}
