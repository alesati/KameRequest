using System.Net.Mail;
using System.Text.RegularExpressions;
using KameRequest.WebService.AccesoDatos.Contratos;
using KameRequest.WebService.LogicaNegocio.Contratos;
using KameRequest.WebService.Modelos;

namespace KameRequest.WebService.LogicaNegocio.Servicios;

// Reglas antes de permitir que el portal externo consulte o registre datos
public class SolicitudesExternasServicio(ISolicitudesExternasRepositorio repositorio) : ISolicitudesExternasServicio
{
    private static readonly Regex FormatoCedulaJuridica = new("^3-\\d{3}-\\d{6}$", RegexOptions.Compiled);

    // pasa al portal solo los datos que ya estan publicados
    public IEnumerable<SolicitudPublicadaDto> ListarPublicadas() => repositorio.ListarPublicadas();

    // busca el detalle que se muestra cuando se abre una solicitud externa
    public SolicitudDetalleExternoDto? ObtenerDetalle(int idSolicitud) => repositorio.ObtenerDetalle(idSolicitud);

    // revisa reglas del proveedor y de la propuesta antes de guardar
    public ResultadoOperacionDto RegistrarParticipacion(int idSolicitud, ParticipacionSolicitudDto participacion)
    {
        if (!repositorio.ExisteSolicitudPublicada(idSolicitud))
            return new ResultadoOperacionDto { Exito = false, Mensaje = "La solicitud no esta disponible para participar." };

        var proveedorExiste = repositorio.ExisteProveedor(participacion.Proveedor.CedulaJuridica);
        var validacion = ValidarParticipacion(participacion, proveedorExiste);
        if (validacion is not null)
            return new ResultadoOperacionDto { Exito = false, Mensaje = validacion };

        try
        {
            var id = repositorio.RegistrarParticipacion(idSolicitud, participacion);
            return new ResultadoOperacionDto { Exito = true, Id = id, Mensaje = "La participacion se registro correctamente." };
        }
        catch (InvalidOperationException error) when (error.Message == "PARTICIPACION_DUPLICADA")
        {
            return new ResultadoOperacionDto { Exito = false, Mensaje = "El proveedor ya registro una participacion para esta solicitud." };
        }
    }

    // EXT4 indica a wordpress si debe pedir los datos de un proveedor nuevo
    public ConsultaProveedorDto ConsultarProveedor(string cedulaJuridica)
    {
        if (!FormatoCedulaJuridica.IsMatch(cedulaJuridica))
            return new ConsultaProveedorDto { FormatoValido = false, Mensaje = "La cedula juridica debe tener el formato 3-000-000000." };

        var proveedor = repositorio.ObtenerProveedor(cedulaJuridica);
        return proveedor is null
            ? new ConsultaProveedorDto { Mensaje = "Proveedor no encontrado. Complete los datos para crearlo." }
            : new ConsultaProveedorDto { Existe = true, Mensaje = "Proveedor encontrado.", Proveedor = proveedor };
    }

    // valida la cedula y devuelve las participaciones hechas por ese proveedor
    public ResultadoOperacionDto ConsultarParticipaciones(string cedulaJuridica, out IEnumerable<ParticipacionProveedorDto> participaciones)
    {
        participaciones = new List<ParticipacionProveedorDto>();
        if (!FormatoCedulaJuridica.IsMatch(cedulaJuridica))
            return new ResultadoOperacionDto { Exito = false, Mensaje = "La cedula juridica debe tener el formato 3-000-000000." };

        if (!repositorio.ExisteProveedor(cedulaJuridica))
            return new ResultadoOperacionDto { Exito = false, Mensaje = "Proveedor no existe." };

        participaciones = repositorio.ListarParticipacionesProveedor(cedulaJuridica);
        if (!participaciones.Any())
            return new ResultadoOperacionDto { Exito = false, Mensaje = "Datos no encontrados." };

        return new ResultadoOperacionDto { Exito = true, Mensaje = "Consulta realizada correctamente." };
    }

    // concentra las validaciones necesarias antes de insertar datos
    private static string? ValidarParticipacion(ParticipacionSolicitudDto participacion, bool proveedorExiste)
    {
        var proveedor = participacion.Proveedor;
        if (!FormatoCedulaJuridica.IsMatch(proveedor.CedulaJuridica))
            return "La cedula juridica debe tener el formato 3-000-000000.";

        if (!proveedorExiste && (string.IsNullOrWhiteSpace(proveedor.NombreProveedor) ||
                                 string.IsNullOrWhiteSpace(proveedor.NombreRepresentante) ||
                                 string.IsNullOrWhiteSpace(proveedor.Telefono) ||
                                 string.IsNullOrWhiteSpace(proveedor.Correo)))
            return "Para crear el proveedor debe indicar nombre, representante, telefono y correo.";

        try
        {
            if (!proveedorExiste)
                _ = new MailAddress(proveedor.Correo);
        }
        catch (FormatException)
        {
            return "El correo del proveedor no tiene un formato valido.";
        }

        if (participacion.Desgloses.Count == 0)
            return "Debe indicar al menos un desglose para la propuesta.";

        if (participacion.Desgloses.Any(d => d.Mes is < 1 or > 12 || d.Anio < 1000 ||
                                             d.Horas < 0 || d.Monto < 0 ||
                                             d.PorcentajeCobro is < 0 or > 100 ||
                                             string.IsNullOrWhiteSpace(d.Observaciones)))
            return "Todos los datos del desglose son requeridos y deben ser validos.";

        if (participacion.Desgloses.Sum(d => d.PorcentajeCobro) > 100)
            return "El porcentaje total de cobro no puede ser mayor a 100%.";

        return null;
    }
}
