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
            // Msvm_ComputerSystem.EnabledState. 전이 값(4 Shutting Down, 10 Starting, 32772 Migrating, 32779 FastSaved,
            // 32780 FastSaving)도 어휘에 둔다(BL-0014). inventory 는 한 행이라도 unknown 이면 전체를 거절하므로 Hyper-V 가
            // 실제로 돌려주는 값은 unknown 으로 떨어지면 안 된다. 0 Unknown 과 1 Other 만 unknown 이다.
            return Convert.ToInt32(value, CultureInfo.InvariantCulture) switch
            {
                2 => "running",
                3 => "stopped",
                4 => "stopping",
                6 => "saved",
                9 => "paused",
                10 => "starting",
                32768 => "paused",
                32769 => "saved",
                32770 => "starting",
                32772 => "running",
                32773 => "saving",
                32774 => "stopping",
                32776 => "pausing",
                32777 => "resuming",
                32779 => "saved",
                32780 => "saving",
                _ => "unknown"
            };
        }
        catch
        {
            return "unknown";
        }
    }

    // vm.create 가 돌아온 직후 새 VM 의 EnabledState 가 잠시 비거나 알 수 없는 값일 수 있다(BL-0014, ADR-0016 통합 run
    // 20261009155949). 쓰기 작업은 돌아가기 전에 inventory 가 읽을 수 있는 state 를 bounded 로 기다린다. 끝까지 unknown 이면
    // false 를 돌려주고 호출자가 step 에 남긴다. 시험은 readState 와 wait 를 주입한다.
    public static bool WaitForKnownState(
        Func<string> readState,
        int attempts,
        TimeSpan interval,
        CancellationToken cancellationToken,
        Func<TimeSpan, CancellationToken, bool>? wait = null)
    {
        ArgumentNullException.ThrowIfNull(readState);
        Func<TimeSpan, CancellationToken, bool> delay = wait ?? ((span, token) => token.WaitHandle.WaitOne(span));
        for (var attempt = 0; attempt < attempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.Equals(readState(), "unknown", StringComparison.Ordinal))
            {
                return true;
            }

            if (attempt + 1 < attempts && delay(interval, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        return false;
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
