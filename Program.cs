using KameRequest.Clases;
using KameRequest.Repository;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// habilita Razor Pages para usar las pantallas cshtml y sus PageModels
builder.Services.AddRazorPages();

// cada solicitud web recibe su propia conexion para trabajar con MySql
builder.Services.AddScoped<IDbConnectionFactory, MySqlConnectionFactory>();

// repositorios: aqui estan separadas las consultas SQL de cada modulo
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<BitacoraRepository>();
builder.Services.AddScoped<RepresentanteRepository>();
builder.Services.AddScoped<SolicitudRepository>();
builder.Services.AddScoped<DesgloseRepository>();
builder.Services.AddScoped<TareaRepository>();
builder.Services.AddScoped<ReporteRepository>();
builder.Services.AddScoped<EstadoRepository>();

// clases con reglas del sistema que usan las pantallas
builder.Services.AddScoped<UsuarioSeguridad>();
builder.Services.AddScoped<BitacoraRegistro>();
builder.Services.AddScoped<NotificacionesCorreos>();
builder.Services.AddScoped<VencimientoClase>();

// proceso que revisa vencimientos mientras el sistema esta encendido
builder.Services.AddHostedService<ProcesoVencimientoBackgroundService>();

// usa cookies para saber cual usuario ya inicio sesion
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(opciones =>
{
    // pantalla a la que se envia una persona sin sesion
    opciones.LoginPath = "/Cuenta/IniciarSesion";
    opciones.AccessDeniedPath = "/Cuenta/IniciarSesion";

    // agrega un mensaje cuando alguien intenta abrir una pagina protegida por URL
    opciones.Events.OnRedirectToLogin = contexto =>
    {
        contexto.Response.Redirect(contexto.RedirectUri + "&mensaje=sesion");
        return Task.CompletedTask;
    };
});
// usa la cultura de Costa Rica para montos, fechas y numeros en todo el sistema
var culturaCostaRica = new CultureInfo("es-CR");
CultureInfo.DefaultThreadCurrentCulture = culturaCostaRica;
CultureInfo.DefaultThreadCurrentUICulture = culturaCostaRica;
// termina la configuracion y crea la aplicacion para atender solicitudes
var app = builder.Build();

// en produccion se ocultan detalles tecnicos y se muestra la pagina Error
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

// al iniciar revisa si ya existe algun usuario y crea admin solo si hace falta
if (!string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("MySql")) && !string.IsNullOrWhiteSpace(app.Configuration["Seguridad:ClaveAes"]))
{
    using var alcance = app.Services.CreateScope();
    alcance.ServiceProvider.GetRequiredService<UsuarioSeguridad>().CrearAdministradorInicialSiHaceFalta();
}

app.UseStaticFiles();// permite cargar css, javascript, imagenes y logo desde wwwroot


app.UseRouting();// prepara las rutas y revisa la cookie antes de abrir una pagina
app.UseAuthentication();//lee la cookie que envía el navegador y reconstruye el usuario actual
app.UseAuthorization();//revisa si la página exige una sesión válida


app.Use(async (contexto, siguiente) =>// si una pantalla tiene un error tecnico se intenta guardar en la bitacora
{
    try
    {
        await siguiente();
    }
    catch (Exception excepcion)
    {
        try
        {
            contexto.RequestServices.GetRequiredService<BitacoraRegistro>()
                .RegistrarErrorTecnico(contexto.User, contexto.Request.Path, excepcion);
        }
        catch
        {
            // si falla la bitacora no se oculta el error original
        }

        throw;
    }
});
app.MapRazorPages();
app.Run();
