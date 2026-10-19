using KameRequest.Models;

namespace KameRequest.Services.Abstract;

// contrato para que las pantallas o una API usen usuarios sin conocer el repositorio
public interface IUsuarioService
{
    void CrearAdministradorInicialSiHaceFalta();
    ResultadoAcceso ValidarAcceso(InicioSesion datos, out Usuario? usuario);
    int ContarUsuarios();
    IEnumerable<Usuario> ListarUsuarios(int pagina, int tamanoPagina);
    IEnumerable<Usuario> ListarUsuarios();
    UsuarioFormulario? ObtenerUsuario(int idUsuario);
    bool CrearUsuario(UsuarioFormulario datos, out string? error);
    bool ActualizarUsuario(UsuarioFormulario datos, out string? error);
    bool CambiarEstadoUsuario(int idUsuario, string estado, out string? error);
    bool EliminarUsuario(int idUsuario, out string? error);
}
