using Dapper;
using KameRequest.Models;
using KameRequest.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Estados;

[Authorize]
public class CrearModel(IConfiguration configuracion, BitacoraRegistro bitacora) : PageModel
{
    [BindProperty]
    public Estado Estado{ get; set; } = new();

    public IActionResult OnPost()
    {
        using var conexion = CrearConexion();
        conexion.Execute("""
            INSERT INTO estadossolicitud (Nombre)
            VALUES (@Nombre);
            """, Estado);

        Estado.idEstado = conexion.ExecuteScalar<int>("SELECT LAST_INSERT_ID();");
        var nuevo = ObtenerEstado(conexion, Estado.idEstado)!;
        bitacora.RegistrarCreacion(User, "estado", nuevo);

        TempData["Mensaje"] = "El estado se creo correctamente.";
        return RedirectToPage("Index");
    }

    private static Estado? ObtenerEstado(MySqlConnection conexion, int idEstado) =>
        conexion.QuerySingleOrDefault<Estado>("""
            SELECT IdEstado, Nombre
            FROM estadossolicitud
            WHERE IdEstado = @idestado;
            """, new { idEstado});

    private MySqlConnection CrearConexion() =>
        new(configuracion.GetConnectionString("MySql"));
}
