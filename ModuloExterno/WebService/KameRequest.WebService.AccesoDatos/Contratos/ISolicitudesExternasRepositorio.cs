using KameRequest.WebService.Modelos;

namespace KameRequest.WebService.AccesoDatos.Contratos;

// define las consultas y registros que usa el portal externo en la base de datos
public interface ISolicitudesExternasRepositorio
{
    // obtiene solo las solicitudes que se pueden mostrar fuera de KameRequest
    IEnumerable<SolicitudPublicadaDto> ListarPublicadas();

    // busca toda la informacion publica de una solicitud
    SolicitudDetalleExternoDto? ObtenerDetalle(int idSolicitud);

    // confirma que una solicitud sigue publicada antes de recibir una participacion
    bool ExisteSolicitudPublicada(int idSolicitud);

    // consulta las participaciones que pertenecen a un proveedor
    IEnumerable<ParticipacionProveedorDto> ListarParticipacionesProveedor(string cedulaJuridica);

    // revisa si el proveedor ya estaba creado
    bool ExisteProveedor(string cedulaJuridica);

    // obtiene la informacion guardada de un proveedor
    ProveedorDto? ObtenerProveedor(string cedulaJuridica);

    // guarda la participacion y sus lineas en una misma transaccion
    int RegistrarParticipacion(int idSolicitud, ParticipacionSolicitudDto participacion);

    // deja un registro cuando sucede un error tecnico del portal
    void RegistrarErrorTecnico(string descripcion);
}
