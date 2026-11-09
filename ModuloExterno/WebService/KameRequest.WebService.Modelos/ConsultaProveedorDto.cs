namespace KameRequest.WebService.Modelos;

// respuesta usada para saber si el proveedor ya existe antes de participar
public class ConsultaProveedorDto
{
    // indica si la cedula cumple el formato solicitado
    public bool FormatoValido { get; set; } = true;

    // indica si se encontro el proveedor en la base de datos
    public bool Existe { get; set; }

    // devuelve un mensaje que el portal puede mostrar al proveedor
    public string Mensaje { get; set; } = string.Empty;

    // contiene los datos cuando el proveedor ya existia
    public ProveedorDto? Proveedor { get; set; }
}
