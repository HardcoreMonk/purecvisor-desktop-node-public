namespace DesktopNode.Api;

public sealed partial class DesktopNodeApiRequestProcessor
{
    public async Task RunWorkerLoopAsync(
        CancellationToken cancellationToken,
        int workerCount = 1,
        TimeSpan? idleDelay = null)
    {
        var delay = idleDelay ?? TimeSpan.FromMilliseconds(250);
        while (!cancellationToken.IsCancellationRequested)
        {
            var processed = false;
            // The Desktop Node API runtime currently runs one background mutation worker.
            var boundedWorkerCount = Math.Clamp(workerCount, 1, 1);
            for (var index = 0; index < boundedWorkerCount; index++)
            {
                DesktopNodeApiWorkerTickResult tick;
                try
                {
                    tick = await jobWorker.ProcessOneQueuedJobAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch
                {
                    break;
                }

                if (tick.Error is not null)
                {
                    // A NotCommitted start leaves the job durably queued and is safe to
                    // reevaluate after the normal poll delay. Completion uncertainty sets
                    // the runtime load block, so reevaluation cannot replay the provider.
                    break;
                }

                if (!tick.Processed)
                {
                    break;
                }

                processed = true;
            }

            if (processed)
            {
                continue;
            }

            try
            {
                ProcessDueCheckpointSchedules(force: false, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch
            {
            }

            try
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}
