using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Microsoft.Win32.SafeHandles;
using Microsoft.Win32;

namespace DesktopNode.Host;

public sealed record DesktopNodeWindowsServiceSnapshot(
    string ServiceName,
    bool Exists,
    string Status,
    string? BinaryPathName,
    int? Win32ExitCode);

public sealed record DesktopNodeWindowsServiceFailureAction(string Action, TimeSpan Delay);

public sealed record DesktopNodeWindowsServiceConfiguration(
    string ServiceName,
    string DisplayName,
    string Description,
    string BinaryPathName,
    string ServiceAccount,
    bool AutoStart,
    int FailureResetPeriodSeconds,
    IReadOnlyList<DesktopNodeWindowsServiceFailureAction> FailureActions);

public interface IDesktopNodeWindowsServiceController
{
    DesktopNodeWindowsServiceSnapshot Query(string serviceName);
    DesktopNodeWindowsServiceSnapshot Configure(DesktopNodeWindowsServiceConfiguration configuration, TimeSpan timeout);
    DesktopNodeWindowsServiceSnapshot Start(string serviceName, TimeSpan timeout);
    DesktopNodeWindowsServiceSnapshot Stop(string serviceName, TimeSpan timeout);
    DesktopNodeWindowsServiceSnapshot Delete(string serviceName, TimeSpan timeout);
}

public sealed class DesktopNodeWindowsServiceController : IDesktopNodeWindowsServiceController
{
    public DesktopNodeWindowsServiceSnapshot Query(string serviceName)
    {
        try
        {
            using var service = new ServiceController(serviceName);
            var status = MapStatus(service.Status);
            return new DesktopNodeWindowsServiceSnapshot(
                ServiceName: serviceName,
                Exists: true,
                Status: status,
                BinaryPathName: ReadBinaryPathName(serviceName),
                Win32ExitCode: 0);
        }
        catch (InvalidOperationException)
        {
            return Missing(serviceName);
        }
        catch (Win32Exception error) when (error.NativeErrorCode == 1060)
        {
            return Missing(serviceName);
        }
    }

    public DesktopNodeWindowsServiceSnapshot Start(string serviceName, TimeSpan timeout)
    {
        var current = Query(serviceName);
        if (!current.Exists || string.Equals(current.Status, "running", StringComparison.OrdinalIgnoreCase))
        {
            return current;
        }

        try
        {
            using var service = new ServiceController(serviceName);
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            return Query(serviceName);
        }
        catch (Exception error) when (error is InvalidOperationException or Win32Exception or System.ServiceProcess.TimeoutException or System.TimeoutException)
        {
            throw new DesktopNodeWindowsServiceControllerException(
                "PCV_HOST_SERVICE_START_FAILED",
                $"The Desktop Node service could not be started: {error.Message}",
                error);
        }
    }

    public DesktopNodeWindowsServiceSnapshot Configure(DesktopNodeWindowsServiceConfiguration configuration, TimeSpan timeout)
    {
        try
        {
            using var manager = OpenServiceManager();
            using var service = OpenExistingService(manager, configuration.ServiceName, NativeMethods.ServiceAllAccess) ??
                CreateService(manager, configuration);

            if (!NativeMethods.ChangeServiceConfig(
                    service,
                    NativeMethods.ServiceNoChange,
                    configuration.AutoStart ? NativeMethods.ServiceAutoStart : NativeMethods.ServiceDemandStart,
                    NativeMethods.ServiceErrorNormal,
                    configuration.BinaryPathName,
                    null,
                    IntPtr.Zero,
                    null,
                    ServiceAccountOrNull(configuration.ServiceAccount),
                    null,
                    configuration.DisplayName))
            {
                throw NativeControllerException("PCV_HOST_SERVICE_CONFIGURE_FAILED", "The Desktop Node service configuration could not be updated.");
            }

            ApplyDescription(service, configuration.Description);
            ApplyFailureActions(service, configuration);
            return Query(configuration.ServiceName);
        }
        catch (Exception error) when (error is Win32Exception or InvalidOperationException)
        {
            throw new DesktopNodeWindowsServiceControllerException(
                "PCV_HOST_SERVICE_CONFIGURE_FAILED",
                $"The Desktop Node service could not be configured: {error.Message}",
                error);
        }
    }

    public DesktopNodeWindowsServiceSnapshot Stop(string serviceName, TimeSpan timeout)
    {
        var current = Query(serviceName);
        if (!current.Exists || string.Equals(current.Status, "stopped", StringComparison.OrdinalIgnoreCase))
        {
            return current;
        }

        try
        {
            using var service = new ServiceController(serviceName);
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            return Query(serviceName);
        }
        catch (Exception error) when (error is InvalidOperationException or Win32Exception or System.ServiceProcess.TimeoutException or System.TimeoutException)
        {
            throw new DesktopNodeWindowsServiceControllerException(
                "PCV_HOST_SERVICE_STOP_FAILED",
                $"The Desktop Node service could not be stopped: {error.Message}",
                error);
        }
    }

    public DesktopNodeWindowsServiceSnapshot Delete(string serviceName, TimeSpan timeout)
    {
        try
        {
            using var manager = OpenServiceManager();
            using var service = OpenExistingService(manager, serviceName, NativeMethods.ServiceAllAccess);
            if (service is null)
            {
                return Missing(serviceName);
            }

            if (!NativeMethods.DeleteService(service))
            {
                var error = Marshal.GetLastWin32Error();
                if (error != NativeMethods.ErrorServiceMarkedForDelete)
                {
                    throw new Win32Exception(error);
                }
            }

            service.Dispose();
            return WaitForMissing(serviceName, timeout);
        }
        catch (Exception error) when (error is Win32Exception or InvalidOperationException)
        {
            throw new DesktopNodeWindowsServiceControllerException(
                "PCV_HOST_SERVICE_DELETE_FAILED",
                $"The Desktop Node service could not be deleted: {error.Message}",
                error);
        }
    }

    private static DesktopNodeWindowsServiceSnapshot Missing(string serviceName)
    {
        return new DesktopNodeWindowsServiceSnapshot(
            ServiceName: serviceName,
            Exists: false,
            Status: "missing",
            BinaryPathName: null,
            Win32ExitCode: 1060);
    }

    private static string? ReadBinaryPathName(string serviceName)
    {
        using var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}");
        return key?.GetValue("ImagePath") as string;
    }

    private static SafeServiceHandle OpenServiceManager()
    {
        var handle = NativeMethods.OpenSCManager(null, null, NativeMethods.ScManagerAllAccess);
        if (handle.IsInvalid)
        {
            throw NativeControllerException("PCV_HOST_SERVICE_MANAGER_OPEN_FAILED", "The Windows Service Control Manager could not be opened.");
        }

        return handle;
    }

    private static SafeServiceHandle? OpenExistingService(SafeServiceHandle manager, string serviceName, uint desiredAccess)
    {
        var handle = NativeMethods.OpenService(manager, serviceName, desiredAccess);
        if (!handle.IsInvalid)
        {
            return handle;
        }

        var error = Marshal.GetLastWin32Error();
        if (error == NativeMethods.ErrorServiceDoesNotExist)
        {
            handle.Dispose();
            return null;
        }

        handle.Dispose();
        throw new Win32Exception(error);
    }

    private static SafeServiceHandle CreateService(SafeServiceHandle manager, DesktopNodeWindowsServiceConfiguration configuration)
    {
        var handle = NativeMethods.CreateService(
            manager,
            configuration.ServiceName,
            configuration.DisplayName,
            NativeMethods.ServiceAllAccess,
            NativeMethods.ServiceWin32OwnProcess,
            configuration.AutoStart ? NativeMethods.ServiceAutoStart : NativeMethods.ServiceDemandStart,
            NativeMethods.ServiceErrorNormal,
            configuration.BinaryPathName,
            null,
            IntPtr.Zero,
            null,
            ServiceAccountOrNull(configuration.ServiceAccount),
            null);
        if (handle.IsInvalid)
        {
            throw NativeControllerException("PCV_HOST_SERVICE_CREATE_FAILED", "The Desktop Node service could not be created.");
        }

        return handle;
    }

    private static void ApplyDescription(SafeServiceHandle service, string description)
    {
        var info = new NativeMethods.ServiceDescription { Description = description };
        var size = Marshal.SizeOf<NativeMethods.ServiceDescription>();
        var pointer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(info, pointer, fDeleteOld: false);
            if (!NativeMethods.ChangeServiceConfig2(service, NativeMethods.ServiceConfigDescription, pointer))
            {
                throw NativeControllerException("PCV_HOST_SERVICE_DESCRIPTION_FAILED", "The Desktop Node service description could not be updated.");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    private static void ApplyFailureActions(SafeServiceHandle service, DesktopNodeWindowsServiceConfiguration configuration)
    {
        var actions = configuration.FailureActions
            .Select(action => new NativeMethods.ScAction(MapFailureActionType(action.Action), checked((int)action.Delay.TotalMilliseconds)))
            .ToArray();
        var actionsHandle = GCHandle.Alloc(actions, GCHandleType.Pinned);
        var pointer = IntPtr.Zero;
        try
        {
            var info = new NativeMethods.ServiceFailureActions
            {
                ResetPeriod = configuration.FailureResetPeriodSeconds,
                RebootMessage = null,
                Command = null,
                ActionCount = actions.Length,
                Actions = actionsHandle.AddrOfPinnedObject()
            };
            var size = Marshal.SizeOf<NativeMethods.ServiceFailureActions>();
            pointer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(info, pointer, fDeleteOld: false);
            if (!NativeMethods.ChangeServiceConfig2(service, NativeMethods.ServiceConfigFailureActions, pointer))
            {
                throw NativeControllerException("PCV_HOST_SERVICE_FAILURE_POLICY_FAILED", "The Desktop Node service failure policy could not be updated.");
            }
        }
        finally
        {
            if (pointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pointer);
            }

            actionsHandle.Free();
        }
    }

    private static DesktopNodeWindowsServiceSnapshot WaitForMissing(string serviceName, TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        DesktopNodeWindowsServiceSnapshot current;
        do
        {
            current = QueryStatic(serviceName);
            if (!current.Exists)
            {
                return current;
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(500));
        }
        while (DateTimeOffset.UtcNow < deadline);

        return current;
    }

    private static DesktopNodeWindowsServiceSnapshot QueryStatic(string serviceName)
    {
        return new DesktopNodeWindowsServiceController().Query(serviceName);
    }

    private static string? ServiceAccountOrNull(string serviceAccount)
    {
        return string.Equals(serviceAccount, "LocalSystem", StringComparison.OrdinalIgnoreCase) ? null : serviceAccount;
    }

    private static int MapFailureActionType(string action)
    {
        return action.ToLowerInvariant() switch
        {
            "restart" => NativeMethods.ScActionRestart,
            "none" => NativeMethods.ScActionNone,
            _ => throw new InvalidOperationException($"Unsupported service failure action '{action}'.")
        };
    }

    private static Win32Exception NativeControllerException(string code, string message)
    {
        return new Win32Exception(Marshal.GetLastWin32Error(), $"{code}|{message}");
    }

    private static string MapStatus(ServiceControllerStatus status)
    {
        return status switch
        {
            ServiceControllerStatus.ContinuePending => "continue-pending",
            ServiceControllerStatus.Paused => "paused",
            ServiceControllerStatus.PausePending => "pause-pending",
            ServiceControllerStatus.Running => "running",
            ServiceControllerStatus.StartPending => "start-pending",
            ServiceControllerStatus.Stopped => "stopped",
            ServiceControllerStatus.StopPending => "stop-pending",
            _ => status.ToString().ToLowerInvariant()
        };
    }
}

internal sealed class SafeServiceHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private SafeServiceHandle()
        : base(ownsHandle: true)
    {
    }

    protected override bool ReleaseHandle()
    {
        return NativeMethods.CloseServiceHandle(handle);
    }
}

internal static partial class NativeMethods
{
    public const int ErrorServiceDoesNotExist = 1060;
    public const int ErrorServiceMarkedForDelete = 1072;
    public const uint ScManagerAllAccess = 0xF003F;
    public const uint ServiceAllAccess = 0xF01FF;
    public const uint ServiceNoChange = 0xFFFFFFFF;
    public const uint ServiceWin32OwnProcess = 0x00000010;
    public const uint ServiceAutoStart = 0x00000002;
    public const uint ServiceDemandStart = 0x00000003;
    public const uint ServiceErrorNormal = 0x00000001;
    public const int ServiceConfigDescription = 1;
    public const int ServiceConfigFailureActions = 2;
    public const int ScActionNone = 0;
    public const int ScActionRestart = 1;

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern SafeServiceHandle OpenSCManager(string? machineName, string? databaseName, uint desiredAccess);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern SafeServiceHandle OpenService(SafeServiceHandle serviceManager, string serviceName, uint desiredAccess);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern SafeServiceHandle CreateService(
        SafeServiceHandle serviceManager,
        string serviceName,
        string displayName,
        uint desiredAccess,
        uint serviceType,
        uint startType,
        uint errorControl,
        string binaryPathName,
        string? loadOrderGroup,
        IntPtr tagId,
        string? dependencies,
        string? serviceStartName,
        string? password);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ChangeServiceConfig(
        SafeServiceHandle service,
        uint serviceType,
        uint startType,
        uint errorControl,
        string binaryPathName,
        string? loadOrderGroup,
        IntPtr tagId,
        string? dependencies,
        string? serviceStartName,
        string? password,
        string displayName);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ChangeServiceConfig2(SafeServiceHandle service, int infoLevel, IntPtr info);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteService(SafeServiceHandle service);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseServiceHandle(IntPtr handle);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ServiceDescription
    {
        public string Description;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ServiceFailureActions
    {
        public int ResetPeriod;
        public string? RebootMessage;
        public string? Command;
        public int ActionCount;
        public IntPtr Actions;
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ScAction(int type, int delay)
    {
        public readonly int Type = type;
        public readonly int Delay = delay;
    }
}

public sealed class DesktopNodeWindowsServiceControllerException(
    string code,
    string message,
    Exception innerException) : Exception(message, innerException)
{
    public string Code { get; } = code;
}
