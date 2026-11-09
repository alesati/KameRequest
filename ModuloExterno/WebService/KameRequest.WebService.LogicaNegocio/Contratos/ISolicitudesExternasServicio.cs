using KameRequest.WebService.Modelos;

namespace KameRequest.WebService.LogicaNegocio.Contratos;

// operaciones que aplica la logica antes de atender al portal externo
public interface ISolicitudesExternasServicio
{
    // lista las solicitudes publicadas y vigentes
    IEnumerable<SolicitudPublicadaDto> ListarPublicadas();

    // obtiene el detalle visible desde wordpress
    SolicitudDetalleExternoDto? ObtenerDetalle(int idSolicitud);

    // valida y registra la propuesta enviada por un proveedor
    ResultadoOperacionDto RegistrarParticipacion(int idSolicitud, ParticipacionSolicitudDto participacion);

    // indica si el proveedor existe o si se deben pedir sus datos
    ConsultaProveedorDto ConsultarProveedor(string cedulaJuridica);

    // muestra las participaciones anteriores de un proveedor
    ResultadoOperacionDto ConsultarParticipaciones(string cedulaJuridica, out IEnumerable<ParticipacionProveedorDto> participaciones);
}
