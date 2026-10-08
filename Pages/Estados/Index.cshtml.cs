using Dapper;
using KameRequest.Models;
using KameRequest.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Estados;

[Authorize]
public class IndexModel(IConfiguration configuracion, BitacoraRegistro bitacora) : PageModel
{
    private const int RegistrosPorPagina = 10;

    public IReadOnlyList<Estado> Estados { get; private set; } = [];
    public int PaginaActual { get; private set; }
    public int TotalPaginas { get; private set; }

    public void OnGet(int pagina = 1)
    {
        bitacora.RegistrarConsulta(User, "Estados");
        CargarEstados(pagina);
    }

    public IActionResult OnPostEliminar(int idEstado)
    {
        try
        {
            using var conexion = CrearConexion();
            var eliminado = ObtenerEstado(conexion, idEstado);
            if (eliminado is null)
            {
                return RedirectToPage();
            }

            var eliminados = conexion.Execute(
                "DELETE FROM estadossolicitud WHERE IdEstado = @idEstado;",
                new { idEstado });

            if (eliminados == 1)
            {
                bitacora.RegistrarEliminacion(User, "Estado", eliminado);
                TempData["Mensaje"] = "El estado se elimino correctamente.";
            }
        }
        catch (MySqlException excepcion) when (excepcion.Number == 1451)
        {
            TempData["Error"] = "No se puede eliminar un registro con datos relacionados.";
        }

        return RedirectToPage();
    }

    private void CargarEstados(int pagina)
    {
        using var conexion = CrearConexion();
        var totalRegistros = conexion.ExecuteScalar<int>("SELECT COUNT(*) FROM estadossolicitud;");
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)RegistrosPorPagina));
        PaginaActual = Math.Clamp(pagina, 1, TotalPaginas);

        Estados = conexion.Query<Estado>("""
            SELECT idestado, Nombre
            FROM estadossolicitud
            ORDER BY 1,2
            LIMIT @registrosPorPagina OFFSET @desplazamiento;
            """, new
        {
            registrosPorPagina = RegistrosPorPagina,
            desplazamiento = (PaginaActual - 1) * RegistrosPorPagina
        }).ToList();
    }

    private static Estado? ObtenerEstado(MySqlConnection conexion, int idEstado) =>
        conexion.QuerySingleOrDefault<Estado>("""
            SELECT IdEstado, Nombre
            FROM estadossolicitud
            WHERE idestado = @idestado;
            """, new { idEstado});

    private MySqlConnection CrearConexion() =>
        new(configuracion.GetConnectionString("MySql"));
}
