using KameRequest.WebService.Modelos;

namespace KameRequest.WebService.AccesoDatos.Contratos;

// define las consultas y registros que usa el portal externo en la base de datos
public interface ISolicitudesExternasRepositorio
{
   IEnumerable<SolicitudPublicadaDto> ListarPublicadas();
    SolicitudDetalleExternoDto? ObtenerDetalle(int idSolicitud);

    bool ExisteSolicitudPublicada(int idSolicitud);

    IEnumerable<ParticipacionProveedorDto> ListarParticipacionesProveedor(string cedulaJuridica);

    bool ExisteProveedor(string cedulaJuridica);

    ProveedorDto? ObtenerProveedor(string cedulaJuridica);

    int RegistrarParticipacion(int idSolicitud, ParticipacionSolicitudDto participacion);
    void RegistrarErrorTecnico(string descripcion);
}
