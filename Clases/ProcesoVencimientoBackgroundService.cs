
namespace KameRequest.Clases;

// Ejecuta periodicamente la revision de vencimientos sin requerir un usuario autenticado.
public class ProcesoVencimientoBackgroundService(IServiceScopeFactory fabricaAlcances) : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken detener)
    {
        EjecutarProceso();

        using var temporizador = new PeriodicTimer(Intervalo);
        while (await temporizador.WaitForNextTickAsync(detener))
        {
            EjecutarProceso();
        }
    }

    private void EjecutarProceso()
    {
        using var alcance = fabricaAlcances.CreateScope();
        try
        {
            alcance.ServiceProvider.GetRequiredService<VencimientoClase>()
                .ActualizarSolicitudesVencidas();
        }
        catch (Exception excepcion)
        {
            try
            {
                alcance.ServiceProvider.GetRequiredService<BitacoraRegistro>()
                    .RegistrarErrorTecnicoSistema("proceso de vencimiento", excepcion);
            }
            catch
            {
                // Si la bitacora no esta disponible, se conserva la ejecucion del proceso.
            }
        }
    }
}
