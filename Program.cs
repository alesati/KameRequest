using KameRequest.Clases;
using KameRequest.Repository;
using KameRequest.Repository.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddScoped<IDbConnectionFactory, MySqlConnectionFactory>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<BitacoraRepository>();
builder.Services.AddScoped<RepresentanteRepository>();
builder.Services.AddScoped<SolicitudRepository>();
builder.Services.AddScoped<DesgloseRepository>();
builder.Services.AddScoped<TareaRepository>();
builder.Services.AddScoped<ReporteRepository>();
builder.Services.AddScoped<EstadoRepository>();
builder.Services.AddScoped<UsuarioSeguridad>();
builder.Services.AddScoped<BitacoraRegistro>();
builder.Services.AddScoped<NotificacionesCorreos>();
builder.Services.AddScoped<VencimientoClase>();
builder.Services.AddHostedService<ProcesoVencimientoBackgroundService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(opciones =>
{
    opciones.LoginPath = "/Cuenta/IniciarSesion";
    opciones.AccessDeniedPath = "/Cuenta/IniciarSesion";
    opciones.Events.OnRedirectToLogin = contexto =>
    {
        contexto.Response.Redirect(contexto.RedirectUri + "&mensaje=sesion");
        return Task.CompletedTask;
    };
});

var app = builder.Build();
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

if (!string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("MySql")) && !string.IsNullOrWhiteSpace(app.Configuration["Seguridad:ClaveAes"]))
{
    using var alcance = app.Services.CreateScope();
    alcance.ServiceProvider.GetRequiredService<UsuarioSeguridad>().CrearAdministradorInicialSiHaceFalta();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (contexto, siguiente) =>
{
    try { await siguiente(); }
    catch (Exception excepcion)
    {
        try { contexto.RequestServices.GetRequiredService<BitacoraRegistro>().RegistrarErrorTecnico(contexto.User, contexto.Request.Path, excepcion); }
        catch { }
        throw;
    }
});
app.MapRazorPages();
app.Run();


