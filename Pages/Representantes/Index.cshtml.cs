using Dapper;
using KameRequest.Models;
using KameRequest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Representantes;

[Authorize]
public class IndexModel(IConfiguration configuracion, BitacoraService bitacora) : PageModel
{
    private const int RegistrosPorPagina = 10;

    public IReadOnlyList<Representante> Representantes { get; private set; } = [];
    public int PaginaActual { get; private set; }
    public int TotalPaginas { get; private set; }

    public void OnGet(int pagina = 1)
    {
        bitacora.RegistrarConsulta(User, "representantes");
        CargarRepresentantes(pagina);
    }

    public IActionResult OnPostEliminar(int idRepresentante)
    {
        try
        {
            using var conexion = CrearConexion();
            var eliminado = ObtenerRepresentante(conexion, idRepresentante);
            if (eliminado is null)
            {
                return RedirectToPage();
            }

            var eliminados = conexion.Execute(
                "DELETE FROM Representantes WHERE IdRepresentante = @idRepresentante;",
                new { idRepresentante });

            if (eliminados == 1)
            {
                bitacora.RegistrarEliminacion(User, "representante", eliminado);
                TempData["Mensaje"] = "El representante se elimino correctamente.";
            }
        }
        catch (MySqlException excepcion) when (excepcion.Number == 1451)
        {
            TempData["Error"] = "No se puede eliminar un registro con datos relacionados.";
        }

        return RedirectToPage();
    }

    private void CargarRepresentantes(int pagina)
    {
        using var conexion = CrearConexion();
        var totalRegistros = conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM Representantes;");
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)RegistrosPorPagina));
        PaginaActual = Math.Clamp(pagina, 1, TotalPaginas);

        Representantes = conexion.Query<Representante>("""
            SELECT IdRepresentante, Nombre, Email
            FROM Representantes
            ORDER BY Nombre, IdRepresentante
            LIMIT @registrosPorPagina OFFSET @desplazamiento;
            """, new
        {
            registrosPorPagina = RegistrosPorPagina,
            desplazamiento = (PaginaActual - 1) * RegistrosPorPagina
        }).ToList();
    }

    private static Representante? ObtenerRepresentante(MySqlConnection conexion, int idRepresentante) =>
        conexion.QuerySingleOrDefault<Representante>("""
            SELECT IdRepresentante, Nombre, Email
            FROM Representantes
            WHERE IdRepresentante = @idRepresentante;
            """, new { idRepresentante });

    private MySqlConnection CrearConexion() =>
        new(configuracion.GetConnectionString("MySql"));
}
