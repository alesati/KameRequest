using Dapper;
using KameRequest.Models;
using KameRequest.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Estados;

[Authorize]
public class EditarModel(IConfiguration configuracion, BitacoraRegistro bitacora) : PageModel
{
    [BindProperty]
    public Estado Estado { get; set; } = new();

    public IActionResult OnGet(int id)
    {
        using var conexion = CrearConexion();
        Estado = ObtenerEstado(conexion, id) ?? new Estado();
        return Estado.idEstado== 0 ? NotFound() : Page();
    }

    public IActionResult OnPost()
    {
        using var conexion = CrearConexion();
        var anterior = ObtenerEstado(conexion, Estado.idEstado);
        if (anterior is null)
        {
            return NotFound();
        }

        conexion.Execute("""
            UPDATE estadossolicitud
            SET Nombre = @Nombre
            WHERE IdEstado = @IdEstado;
            """, Estado);

        var actual = ObtenerEstado(conexion, Estado.idEstado)!;
        bitacora.RegistrarActualizacion(User, "Estado", anterior, actual);

        TempData["Mensaje"] = "El Estado se actualizo correctamente.";
        return RedirectToPage("Index");
    }

    private static Estado? ObtenerEstado(MySqlConnection conexion, int idEstado) =>
        conexion.QuerySingleOrDefault<Estado>("""
            SELECT IdEstado, Nombre
            FROM estadossolicitud
            WHERE IdEstado = @idEstado;
            """, new { idEstado });

    private MySqlConnection CrearConexion() =>
        new(configuracion.GetConnectionString("MySql"));
}
