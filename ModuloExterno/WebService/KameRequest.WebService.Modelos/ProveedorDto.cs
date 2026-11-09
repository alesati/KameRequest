namespace KameRequest.WebService.Modelos;

// Datos del proveedor que participa desde el portal externo
public class ProveedorDto
{
    public string CedulaJuridica { get; set; } = string.Empty;
    public string NombreProveedor { get; set; } = string.Empty;
    public string NombreRepresentante { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
}
