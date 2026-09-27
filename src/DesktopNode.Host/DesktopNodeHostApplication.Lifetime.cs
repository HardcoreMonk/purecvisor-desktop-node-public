namespace DesktopNode.Host;

public sealed partial class DesktopNodeHostApplication
{
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        cancellation.Cancel();
        foreach (var binding in listeners)
        {
            if (binding.Listener.IsListening)
            {
                binding.Listener.Stop();
            }

            binding.Listener.Close();
        }

        try
        {
            Task.WaitAll(loopTasks.ToArray(), TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
        }

        var requestSnapshot = requestTasks.Values.ToArray();
        try
        {
            Task.WaitAll(requestSnapshot, TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
        }

        requestAdmission?.Dispose();
        cancellation.Dispose();
    }
}
