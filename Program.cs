using KameRequest.Clases;
using KameRequest.Repository;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages(); // Habilita las paginas Razor del proyecto
// Registra los repositorios para que el acceso a SQL Server este separado de las paginas Razor.
builder.Services.AddScoped<IDbConnectionFactory, SqlServerConnectionFactory>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<BitacoraRepository>();
builder.Services.AddScoped<RepresentanteRepository>();
builder.Services.AddScoped<SolicitudRepository>();
builder.Services.AddScoped<DesgloseRepository>();
builder.Services.AddScoped<TareaRepository>();
builder.Services.AddScoped<ReporteRepository>();
builder.Services.AddScoped<UsuarioSeguridad>(); // La clase se puede usar en las paginas para trabajar usuarios y seguridad
builder.Services.AddScoped<BitacoraRegistro>(); // Clase que guarda las acciones importantes y errores tecnicos
builder.Services.AddScoped<VencimientoClase>(); // Clase que revisa las solicitudes que deben pasar a vencida
builder.Services.AddHostedService<ProcesoVencimientoBackgroundService>(); // Proceso que ejecuta la revision de vencimientos
builder.Services// Se usa una cookie para recordar la sesion del usuario
.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(opciones =>
{
    // Si no hay sesion o no tiene permiso se envia al login
    opciones.LoginPath = "/Cuenta/IniciarSesion";
    opciones.AccessDeniedPath = "/Cuenta/IniciarSesion";
});
var app = builder.Build();
// Fuera de desarrollo evita mostrar el detalle tecnico de los errores
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// Solo crea la cuenta inicial cuando ya existe una configuracion de base de datos
if (!string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("SqlServer")) && !string.IsNullOrWhiteSpace(app.Configuration["Seguridad:ClaveAes"]))
{
    // El alcance permite usar la clase registrada antes de atender solicitudes
    using var alcance = app.Services.CreateScope();
    alcance.ServiceProvider.GetRequiredService<UsuarioSeguridad>().CrearAdministradorInicialSiHaceFalta();
}

app.UseStaticFiles(); // Da acceso a css, imagenes, javascript y otros archivos de wwwroot
app.UseRouting(); // Permite que el sistema encuentre la pagina segun la direccion
app.UseAuthentication(); // Primero revisa si existe una sesion y luego los permisos de la pagina
app.UseAuthorization(); // Publica las rutas que se crean con las paginas Razor
app.Use(async (contexto, siguiente) =>
{
    try
    {
        await siguiente();
    }
    catch (Exception excepcion)
    {
        try
        {
            var bitacora = contexto.RequestServices.GetRequiredService<BitacoraRegistro>();
            bitacora.RegistrarErrorTecnico(contexto.User, contexto.Request.Path, excepcion);
        }
        catch
        {
            // Si la bitacora no esta disponible, se conserva el error original.
        }

        throw;
    }
});
app.MapRazorPages(); // Mantiene la aplicacion ejecutandose y atendiendo solicitudes
app.Run();

