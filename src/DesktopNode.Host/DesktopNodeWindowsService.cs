using Microsoft.Extensions.Hosting;

namespace DesktopNode.Host;

internal sealed class DesktopNodeWindowsService(DesktopNodeHostOptions options) : BackgroundService
{
    private DesktopNodeHostApplication? application;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        application = await DesktopNodeHostApplication.StartAsync(options).ConfigureAwait(false);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        application?.Dispose();
        application = null;
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    public override void Dispose()
    {
        application?.Dispose();
        application = null;
        base.Dispose();
    }
}
