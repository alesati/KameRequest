using Dapper;
using KameRequest.Models;
using KameRequest.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace KameRequest.Pages.Representantes;

[Authorize]
public class EditarModel(IConfiguration configuracion, BitacoraRegistro bitacora) : PageModel
{
    [BindProperty]
    public Representante Representante { get; set; } = new();

    public IActionResult OnGet(int id)
    {
        using var conexion = CrearConexion();
        Representante = ObtenerRepresentante(conexion, id) ?? new Representante();
        return Representante.IdRepresentante == 0 ? NotFound() : Page();
    }

    public IActionResult OnPost()
    {
        using var conexion = CrearConexion();
        var anterior = ObtenerRepresentante(conexion, Representante.IdRepresentante);
        if (anterior is null)
        {
            return NotFound();
        }

        conexion.Execute("""
            UPDATE Representantes
            SET Nombre = @Nombre,
                Email = @Email
            WHERE IdRepresentante = @IdRepresentante;
            """, Representante);

        var actual = ObtenerRepresentante(conexion, Representante.IdRepresentante)!;
        bitacora.RegistrarActualizacion(User, "representante", anterior, actual);

        TempData["Mensaje"] = "El representante se actualizo correctamente.";
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
