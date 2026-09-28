using KameRequest.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddScoped<UsuarioSeguridadService>();
builder.Services.AddScoped<BitacoraService>();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Cuenta/IniciarSesion";
        opciones.AccessDeniedPath = "/Cuenta/IniciarSesion";
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// Solo crea la cuenta inicial cuando ya existe una configuracion de base de datos
if (!string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("MySql")) &&
    !string.IsNullOrWhiteSpace(app.Configuration["Seguridad:ClaveAes"]))
{
    using var alcance = app.Services.CreateScope();
    alcance.ServiceProvider.GetRequiredService<UsuarioSeguridadService>()
        .CrearAdministradorInicialSiHaceFalta();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (contexto, siguiente) =>
{
    try
    {
        await siguiente();
    }
    catch (Exception ex)
    {
        try
        {
            using var alcance = contexto.RequestServices.CreateScope();
            var bitacora = alcance.ServiceProvider.GetRequiredService<BitacoraService>();
            bitacora.RegistrarErrorTecnico(contexto.User, contexto.Request.Path, ex);
        }
        catch
        {
        }

        throw;
    }
});
app.MapRazorPages();
app.Run();


