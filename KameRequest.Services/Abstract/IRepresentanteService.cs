using KameRequest.Models;

namespace KameRequest.Services.Abstract;

// contrato para administrar los representantes de las solicitudes
public interface IRepresentanteService
{
    IReadOnlyList<Representante> Listar();
    IReadOnlyList<Representante> ObtenerPagina(int pagina, int tamano);
    int Contar();
    Representante? Obtener(int id);
    int Crear(Representante representante);
    void Actualizar(Representante representante);
    int Eliminar(int id);
}
