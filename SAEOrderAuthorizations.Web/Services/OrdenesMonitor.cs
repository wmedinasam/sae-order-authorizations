using Microsoft.EntityFrameworkCore;
using SAEOrderAuthorizations.Web.Data;

namespace SAEOrderAuthorizations.Web.Services;

// Cada ciclo avisa a las listas de órdenes abiertas para que recarguen. El intervalo sale de WIS_CONFIGURATION.
// Singleton a propósito (no termina en Service): un solo temporizador para toda la app, no uno por usuario.
public class OrdenesMonitor(IServiceScopeFactory scopes, ILogger<OrdenesMonitor> logger) : BackgroundService
{
    public const string ClaveIntervalo = "OrdenesRefreshSegundos";
    private const int SegundosDefault = 30;
    private const int SegundosMinimo = 5;

    public event Func<Task>? Refrescar;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Se lee en cada ciclo: cambiar el intervalo en Configuración aplica sin reiniciar la app.
            await Task.Delay(TimeSpan.FromSeconds(await LeerIntervaloAsync(stoppingToken)), stoppingToken);
            Notificar();
        }
    }

    private async Task<int> LeerIntervaloAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var valor = await db.Configuraciones
                .Where(c => c.Key == ClaveIntervalo && c.Estado)
                .Select(c => c.Value)
                .FirstOrDefaultAsync(ct);
            return int.TryParse(valor, out var s) ? Math.Max(s, SegundosMinimo) : SegundosDefault;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "No se pudo leer {Clave}; se usan {Segundos} s.", ClaveIntervalo, SegundosDefault);
            return SegundosDefault;
        }
    }

    // Cada handler corre sin esperar: un circuito lento o ya cerrado no retrasa el temporizador ni a los demás.
    private void Notificar()
    {
        if (Refrescar is null) return;
        foreach (Func<Task> handler in Refrescar.GetInvocationList())
        {
            _ = EjecutarAsync(handler);
        }
    }

    private async Task EjecutarAsync(Func<Task> handler)
    {
        try
        {
            await handler();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo refrescar la lista de órdenes.");
        }
    }
}
