using KameRequest.Clases;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddScoped<UsuarioSeguridad>();
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
    alcance.ServiceProvider.GetRequiredService<UsuarioSeguridad>()
        .CrearAdministradorInicialSiHaceFalta();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
