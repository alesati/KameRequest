using Dapper;
using KameRequest.Models;
using KameRequest.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Representantes;

[Authorize]
public class CrearModel(IConfiguration configuracion, BitacoraRegistro bitacora) : PageModel
{
    [BindProperty]
    public Representante Representante { get; set; } = new();

    public IActionResult OnPost()
    {
        using var conexion = CrearConexion();
        conexion.Execute("""
            INSERT INTO Representantes (Nombre, Email)
            VALUES (@Nombre, @Email);
            """, Representante);

        Representante.IdRepresentante = conexion.ExecuteScalar<int>("SELECT LAST_INSERT_ID();");
        var nuevo = ObtenerRepresentante(conexion, Representante.IdRepresentante)!;
        bitacora.RegistrarCreacion(User, "representante", nuevo);

        TempData["Mensaje"] = "El representante se creo correctamente.";
        return RedirectToPage("Index");
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
