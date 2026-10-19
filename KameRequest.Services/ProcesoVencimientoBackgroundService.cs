using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KameRequest.Services.Abstract;

namespace KameRequest.Services;

// Ejecuta periodicamente la revision de vencimientos sin requerir un usuario autenticado.
public class ProcesoVencimientoBackgroundService(IServiceScopeFactory fabricaAlcances) : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken detener)
    {
        try
        {
            EjecutarProceso();

            using var temporizador = new PeriodicTimer(Intervalo);
            while (await temporizador.WaitForNextTickAsync(detener))
            {
                EjecutarProceso();
            }
        }
        catch (OperationCanceledException) when (detener.IsCancellationRequested)
        {
            // Es normal cuando se detiene o reinicia la aplicacion.
        }
    }

    private void EjecutarProceso()
    {
        using var alcance = fabricaAlcances.CreateScope();

        try
        {
            alcance.ServiceProvider
                .GetRequiredService<IVencimientoService>()
                .ActualizarSolicitudesVencidas();
        }
        catch (Exception excepcion)
        {
            try
            {
                alcance.ServiceProvider
                    .GetRequiredService<IBitacoraService>()
                    .RegistrarErrorTecnicoSistema("proceso de vencimiento", excepcion);
            }
            catch
            {
                // Si la bitacora no esta disponible, se conserva la ejecucion del proceso.
            }
        }
    }
}


