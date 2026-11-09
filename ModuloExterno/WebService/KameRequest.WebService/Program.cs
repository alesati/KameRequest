using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using KameRequest.WebService;
using KameRequest.WebService.AccesoDatos.Contratos;
using KameRequest.WebService.AccesoDatos.Infraestructura;
using KameRequest.WebService.AccesoDatos.Repositorios;
using KameRequest.WebService.LogicaNegocio.Contratos;
using KameRequest.WebService.LogicaNegocio.Servicios;

var builder = WebApplication.CreateBuilder(args);

// registra lo necesario para publicar operaciones SOAP con CoreWCF
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

// toma la conexion que se configura para la base MariaDB de pruebas
var cadenaConexion = builder.Configuration.GetConnectionString("MariaDb")
    ?? throw new InvalidOperationException("No se configuro la conexion MariaDb.");

// conecta las capas del web service para que cada solicitud use repositorio y logica
builder.Services.AddSingleton(new ConexionMariaDb(cadenaConexion));
builder.Services.AddScoped<ISolicitudesExternasRepositorio, SolicitudesExternasRepositorio>();
builder.Services.AddScoped<ISolicitudesExternasServicio, SolicitudesExternasServicio>();
builder.Services.AddScoped<ServicioSolicitudesExternas>();

var app = builder.Build();

// publica el contrato SOAP en una direccion .svc
app.UseServiceModel(servicios =>
{
    servicios.AddService<ServicioSolicitudesExternas>();
    servicios.AddServiceEndpoint<ServicioSolicitudesExternas, IServicioSolicitudesExternas>(
        new BasicHttpBinding(),
        "/ServicioSolicitudesExternas.svc");
});

// permite consultar el WSDL agregando ?wsdl a la direccion del servicio
app.Services.GetRequiredService<ServiceMetadataBehavior>().HttpGetEnabled = true;

app.Run();
