using System.Globalization;
using System.Management;

namespace DesktopNode.HyperV;

internal static class DesktopNodeHyperVWmiCommon
{
    public const string NamespacePath = @"root\virtualization\v2";
    public const string VmQuery = "SELECT * FROM Msvm_ComputerSystem WHERE Description = 'Microsoft Virtual Machine'";
    public const uint Completed = 0;
    public const uint JobStarted = 4096;

    public static ManagementScope CreateScope(bool connect = false)
    {
        var scope = new ManagementScope(NamespacePath);
        if (connect)
        {
            scope.Connect();
        }

        return scope;
    }

    public static ManagementObject? FindVm(ManagementScope scope, string vmName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var searcher = new ManagementObjectSearcher(scope, new ObjectQuery(VmQuery));
        foreach (ManagementObject item in searcher.Get())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var elementName = GetStringProperty(item, "ElementName");
            var name = GetStringProperty(item, "Name");
            if (string.Equals(elementName, vmName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, vmName, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }

            item.Dispose();
        }

        return null;
    }

    public static ManagementObject GetService(ManagementScope scope, string className, CancellationToken cancellationToken)
    {
        return GetSingleObject(scope, $"SELECT * FROM {className}", className, cancellationToken);
    }

    public static ManagementObject GetSingleService(ManagementScope scope, string query, string className, CancellationToken cancellationToken)
    {
        return GetSingleObject(scope, query, className, cancellationToken);
    }

    public static ManagementObject GetSingleObject(ManagementScope scope, string query, string className, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var searcher = new ManagementObjectSearcher(scope, new ObjectQuery(query));
        foreach (ManagementObject item in searcher.Get())
        {
            cancellationToken.ThrowIfCancellationRequested();
            return item;
        }

        throw new DesktopNodeHyperVNativeOperationException(
            "PCV_HYPERV_SERVICE_UNAVAILABLE",
            $"{className} is unavailable.",
            "The native Hyper-V WMI service object was not found.",
            true);
    }

    public static void WaitForMethodResult(ManagementBaseObject outParams, string operation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var returnValue = Convert.ToUInt32(outParams.Properties["ReturnValue"]?.Value, CultureInfo.InvariantCulture);
        if (returnValue == Completed)
        {
            return;
        }

        if (returnValue != JobStarted)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_HYPERV_WMI_METHOD_FAILED",
                $"Native Hyper-V WMI operation '{operation}' failed.",
                $"WMI method returned {returnValue}.",
                true);
        }

        var jobPath = Convert.ToString(outParams.Properties["Job"]?.Value, CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(jobPath))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_HYPERV_WMI_JOB_MISSING",
                $"Native Hyper-V WMI operation '{operation}' did not return a job path.",
                "The WMI method returned JobStarted without a Job reference.",
                true);
        }

        using var job = new ManagementObject(jobPath);
        for (var attempt = 0; attempt < 120; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            job.Get();
            var state = Convert.ToInt32(job.Properties["JobState"]?.Value, CultureInfo.InvariantCulture);
            if (state == 7)
            {
                return;
            }

            if (state is 8 or 9 or 10)
            {
                throw new DesktopNodeHyperVNativeOperationException(
                    "PCV_HYPERV_WMI_JOB_FAILED",
                    $"Native Hyper-V WMI operation '{operation}' job failed.",
                    GetJobFailureDetail(job, state),
                    true);
            }

            cancellationToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
        }

        throw new DesktopNodeHyperVNativeOperationException(
            "PCV_HYPERV_WMI_JOB_TIMEOUT",
            $"Native Hyper-V WMI operation '{operation}' timed out.",
            "The Hyper-V WMI job did not complete within 60 seconds.",
            true);
    }

    public static string MapEnabledState(object? value)
    {
        if (value is null)
        {
            return "unknown";
        }

        try
        {
            return Convert.ToInt32(value, CultureInfo.InvariantCulture) switch
            {
                2 => "running",
                3 => "stopped",
                6 => "saved",
                9 => "paused",
                32768 => "paused",
                32769 => "saved",
                32770 => "starting",
                32773 => "saving",
                32774 => "stopping",
                32776 => "pausing",
                32777 => "resuming",
                _ => "unknown"
            };
        }
        catch
        {
            return "unknown";
        }
    }

    public static string? GetStringProperty(ManagementBaseObject item, string propertyName)
    {
        try
        {
            var value = item.Properties[propertyName]?.Value;
            return value switch
            {
                null => null,
                string[] values => string.Join(Environment.NewLine, values),
                _ => Convert.ToString(value, CultureInfo.InvariantCulture)
            };
        }
        catch (ManagementException)
        {
            return null;
        }
    }

    public static string? GetDateTimeProperty(ManagementBaseObject item, string propertyName)
    {
        try
        {
            var value = item.Properties[propertyName]?.Value;
            return value switch
            {
                null => null,
                DateTime dateTime => dateTime.ToString("o", CultureInfo.InvariantCulture),
                string text when string.IsNullOrWhiteSpace(text) => null,
                string text => ManagementDateTimeConverter
                    .ToDateTime(text)
                    .ToString("o", CultureInfo.InvariantCulture),
                _ => Convert.ToString(value, CultureInfo.InvariantCulture)
            };
        }
        catch (ManagementException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static string GetJobFailureDetail(ManagementBaseObject job, int state)
    {
        var errorCode = Convert.ToString(job.Properties["ErrorCode"]?.Value, CultureInfo.InvariantCulture);
        var errorDescription = Convert.ToString(job.Properties["ErrorDescription"]?.Value, CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(errorDescription)
            ? $"WMI job ended in state {state} with error code {errorCode}."
            : errorDescription;
    }
}
