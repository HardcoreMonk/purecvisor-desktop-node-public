using System.Text;
using System.Text.Json;
using DesktopNode.Host;
using DesktopNode.Host.Ops;

namespace DesktopNode.Host.Tests;
public sealed partial class DesktopNodeHostServiceActionTests
{
    private sealed class RecordingFileAclHardener : IDesktopNodeHostFileAclHardener
    {
        public List<string> Paths { get; } = [];

        public void Harden(string path)
        {
            Paths.Add(Path.GetFullPath(path));
        }
    }

    private static IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot> OwnedTrustStoreSnapshots(bool exists)
    {
        return
        [
            new DesktopNodeWindowsTrustStoreCertificateSnapshot(
                StoreName: "Root",
                StoreLocation: "LocalMachine",
                Thumbprint: "00112233445566778899AABBCCDDEEFF00112233",
                Exists: exists,
                Subject: exists ? "CN=PureCVisor Internal Code Signing Root CA" : null,
                Issuer: exists ? "CN=PureCVisor Internal Code Signing Root CA" : null,
                SerialNumber: exists ? "01" : null,
                Owned: exists),
            new DesktopNodeWindowsTrustStoreCertificateSnapshot(
                StoreName: "TrustedPublisher",
                StoreLocation: "LocalMachine",
                Thumbprint: "AABBCCDDEEFF00112233445566778899AABBCCDD",
                Exists: exists,
                Subject: exists ? "CN=PureCVisor Desktop Node Internal Code Signing" : null,
                Issuer: exists ? "CN=PureCVisor Internal Code Signing Root CA" : null,
                SerialNumber: exists ? "02" : null,
                Owned: exists)
        ];
    }

    private sealed class FakeWindowsServiceController : IDesktopNodeWindowsServiceController
    {
        public DesktopNodeWindowsServiceSnapshot Snapshot { get; init; } = new(
            ServiceName: "PureCVisorDesktopNode",
            Exists: false,
            Status: "missing",
            BinaryPathName: null,
            Win32ExitCode: 1060);

        public DesktopNodeWindowsServiceSnapshot? MutatedSnapshot { get; init; }
        public DesktopNodeWindowsServiceSnapshot? StartedSnapshot { get; init; }
        public DesktopNodeWindowsServiceSnapshot? StoppedSnapshot { get; init; }
        public DesktopNodeWindowsServiceSnapshot? ConfiguredSnapshot { get; init; }
        public DesktopNodeWindowsServiceSnapshot? DeletedSnapshot { get; init; }

        public List<string> Calls { get; } = [];
        public List<DesktopNodeWindowsServiceConfiguration> Configurations { get; } = [];

        public DesktopNodeWindowsServiceSnapshot Query(string serviceName)
        {
            Calls.Add("query");
            return Snapshot;
        }

        public DesktopNodeWindowsServiceSnapshot Start(string serviceName, TimeSpan timeout)
        {
            Calls.Add("start");
            return StartedSnapshot ?? MutatedSnapshot ?? Snapshot;
        }

        public DesktopNodeWindowsServiceSnapshot Stop(string serviceName, TimeSpan timeout)
        {
            Calls.Add("stop");
            return StoppedSnapshot ?? MutatedSnapshot ?? Snapshot;
        }

        public DesktopNodeWindowsServiceSnapshot Configure(DesktopNodeWindowsServiceConfiguration configuration, TimeSpan timeout)
        {
            Calls.Add("configure");
            Configurations.Add(configuration);
            return ConfiguredSnapshot ?? Snapshot;
        }

        public DesktopNodeWindowsServiceSnapshot Delete(string serviceName, TimeSpan timeout)
        {
            Calls.Add("delete");
            return DeletedSnapshot ?? Snapshot;
        }
    }

    private sealed class FakeWindowsEventLogController : IDesktopNodeWindowsEventLogController
    {
        public DesktopNodeWindowsEventLogSnapshot Snapshot { get; init; } = new(
            LogName: "Application",
            SourceName: "PureCVisor Desktop Node",
            Exists: false,
            EventMessageFile: null,
            Owned: false);

        public DesktopNodeWindowsEventLogSnapshot? MutatedSnapshot { get; init; }
        public DesktopNodeWindowsEventLogVolumePolicySnapshot VolumePolicy { get; init; } = new(
            LogName: "Application",
            MaximumSizeBytes: null,
            RetentionPolicy: null,
            VolumeGuarded: false);
        public string WriteStatus { get; init; } = "not-run";

        public List<string> Calls { get; } = [];

        public DesktopNodeWindowsEventLogSnapshot Query(string logName, string sourceName, string expectedEventMessageFile)
        {
            Calls.Add("query");
            return Snapshot;
        }

        public DesktopNodeWindowsEventLogSnapshot Register(string logName, string sourceName, string eventMessageFile)
        {
            Calls.Add("register");
            return MutatedSnapshot ?? Snapshot;
        }

        public DesktopNodeWindowsEventLogSnapshot Remove(string logName, string sourceName, string expectedEventMessageFile)
        {
            Calls.Add("remove");
            return MutatedSnapshot ?? Snapshot;
        }

        public string WriteTestEvent(string logName, string sourceName, int eventId, string message)
        {
            Calls.Add("write-test");
            return WriteStatus;
        }

        public DesktopNodeWindowsEventLogVolumePolicySnapshot QueryVolumePolicy(string logName)
        {
            Calls.Add("volume-guard");
            return VolumePolicy;
        }
    }

    private sealed class FakeWindowsCredentialManagerController : IDesktopNodeWindowsCredentialManagerController
    {
        public DesktopNodeWindowsCredentialManagerProofSnapshot Proof { get; init; } = new(
            Identity: "DESKTOP\\operator",
            CredentialTarget: "PureCVisor/PureCVisorDesktopNode/api-token",
            CredentialWriteStatus: "not-run",
            CredentialReadStatus: "not-run",
            CredentialDeleteStatus: "not-run",
            TokenValueObserved: false,
            NewTokenValueCreated: false);

        public List<string> Calls { get; } = [];
        public Dictionary<string, string> WrittenTokens { get; } = new(StringComparer.Ordinal);

        public DesktopNodeWindowsCredentialManagerProofSnapshot WriteReadDeleteProof(string credentialTarget)
        {
            Calls.Add("write-read-delete-proof");
            return Proof;
        }

        public void WriteToken(string credentialTarget, string token)
        {
            Calls.Add("write-token");
            WrittenTokens[credentialTarget] = token;
        }

        public string ReadToken(string credentialTarget)
        {
            Calls.Add("read-token");
            return WrittenTokens[credentialTarget];
        }

        public void DeleteToken(string credentialTarget)
        {
            Calls.Add("delete-token");
            WrittenTokens.Remove(credentialTarget);
        }
    }

    private sealed class FakeWindowsFirewallController : IDesktopNodeWindowsFirewallController
    {
        public DesktopNodeWindowsFirewallRuleSnapshot Snapshot { get; init; } =
            DesktopNodeWindowsFirewallRuleSnapshot.Missing(
                "PureCVisor Desktop Node Local API LAN",
                "inbound",
                "TCP",
                7777,
                "Private",
                "LocalSubnet");

        public DesktopNodeWindowsFirewallRuleSnapshot? MutatedSnapshot { get; init; }

        public List<string> Calls { get; } = [];

        public DesktopNodeWindowsFirewallRuleSnapshot Query(DesktopNodeWindowsFirewallRuleSpec spec)
        {
            Calls.Add("query");
            return Snapshot;
        }

        public DesktopNodeWindowsFirewallRuleSnapshot Enable(DesktopNodeWindowsFirewallRuleSpec spec)
        {
            Calls.Add("enable");
            return MutatedSnapshot ?? Snapshot;
        }

        public DesktopNodeWindowsFirewallRuleSnapshot Remove(DesktopNodeWindowsFirewallRuleSpec spec)
        {
            Calls.Add("remove");
            return MutatedSnapshot ?? Snapshot;
        }
    }

    private sealed class FakeWindowsTrustStoreController : IDesktopNodeWindowsTrustStoreController
    {
        public IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot> Snapshots { get; init; } =
            MissingTrustStoreSnapshots();

        public IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot>? MutatedSnapshots { get; init; }

        public List<string> Calls { get; } = [];

        public IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot> Query(
            IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSpec> certificates)
        {
            Calls.Add("query");
            return Snapshots;
        }

        public IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot> Install(
            IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSpec> certificates)
        {
            Calls.Add("install");
            return MutatedSnapshots ?? Snapshots;
        }

        public IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot> Remove(
            IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSpec> certificates)
        {
            Calls.Add("remove");
            return MutatedSnapshots ?? Snapshots;
        }
    }

    private sealed class ScriptedWindowsEventLogController : IDesktopNodeWindowsEventLogController
    {
        private readonly Queue<DesktopNodeWindowsEventLogSnapshot> registerSnapshots;
        private readonly Queue<DesktopNodeWindowsEventLogSnapshot> removeSnapshots;
        private readonly DesktopNodeWindowsEventLogSnapshot querySnapshot;
        private readonly DesktopNodeWindowsEventLogVolumePolicySnapshot volumePolicy;

        public ScriptedWindowsEventLogController(
            DesktopNodeWindowsEventLogSnapshot querySnapshot,
            IEnumerable<DesktopNodeWindowsEventLogSnapshot> registerSnapshots,
            IEnumerable<DesktopNodeWindowsEventLogSnapshot> removeSnapshots,
            DesktopNodeWindowsEventLogVolumePolicySnapshot volumePolicy)
        {
            this.querySnapshot = querySnapshot;
            this.registerSnapshots = new Queue<DesktopNodeWindowsEventLogSnapshot>(registerSnapshots);
            this.removeSnapshots = new Queue<DesktopNodeWindowsEventLogSnapshot>(removeSnapshots);
            this.volumePolicy = volumePolicy;
        }

        public List<string> Calls { get; } = [];
        public List<string> Messages { get; } = [];

        public DesktopNodeWindowsEventLogSnapshot Query(string logName, string sourceName, string expectedEventMessageFile)
        {
            Calls.Add("query");
            return querySnapshot;
        }

        public DesktopNodeWindowsEventLogSnapshot Register(string logName, string sourceName, string eventMessageFile)
        {
            Calls.Add("register");
            return registerSnapshots.Count == 0 ? querySnapshot : registerSnapshots.Dequeue();
        }

        public DesktopNodeWindowsEventLogSnapshot Remove(string logName, string sourceName, string expectedEventMessageFile)
        {
            Calls.Add("remove");
            return removeSnapshots.Count == 0 ? querySnapshot : removeSnapshots.Dequeue();
        }

        public string WriteTestEvent(string logName, string sourceName, int eventId, string message)
        {
            Calls.Add("write-test");
            Messages.Add(message);
            return "write-query-pass";
        }

        public DesktopNodeWindowsEventLogVolumePolicySnapshot QueryVolumePolicy(string logName)
        {
            Calls.Add("volume-guard");
            return volumePolicy;
        }
    }

    private sealed class BlockingEventLogWriteController : IDesktopNodeWindowsEventLogController
    {
        private readonly DesktopNodeWindowsEventLogSnapshot snapshot;
        private readonly ManualResetEventSlim block = new(false);

        public BlockingEventLogWriteController(DesktopNodeWindowsEventLogSnapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        public List<string> Calls { get; } = [];

        public DesktopNodeWindowsEventLogSnapshot Query(string logName, string sourceName, string expectedEventMessageFile)
        {
            Calls.Add("query");
            return snapshot;
        }

        public DesktopNodeWindowsEventLogSnapshot Register(string logName, string sourceName, string eventMessageFile)
        {
            Calls.Add("register");
            return snapshot;
        }

        public DesktopNodeWindowsEventLogSnapshot Remove(string logName, string sourceName, string expectedEventMessageFile)
        {
            Calls.Add("remove");
            return snapshot;
        }

        public string WriteTestEvent(string logName, string sourceName, int eventId, string message)
        {
            Calls.Add("write-test");
            block.Wait();
            return "write-query-pass";
        }

        public DesktopNodeWindowsEventLogVolumePolicySnapshot QueryVolumePolicy(string logName)
        {
            Calls.Add("volume-guard");
            return new DesktopNodeWindowsEventLogVolumePolicySnapshot(
                LogName: logName,
                MaximumSizeBytes: 20 * 1024 * 1024,
                RetentionPolicy: "overwrite-as-needed",
                VolumeGuarded: true);
        }
    }

    private static object? DescriptorValue(object descriptor, string propertyName)
    {
        return descriptor.GetType().GetProperty(propertyName)?.GetValue(descriptor);
    }

    private static DesktopNodeHostOptions OperationFamilyOptions(string action)
    {
        var options = NativeActionOptions.WithAction(action);

        if (action is "configure-installed" or "repair-installed" or "remove-installed" or
            "config-migration-apply" or "job-store-migration-apply" or
            "service-token-rotation-revoke" or "credential-manager-default-transition")
        {
            options = options.WithDataRoot("C:\\ProgramData\\PureCVisor\\desktop-node");
        }

        if (action == "data-root-remove")
        {
            options = options.WithDataRoot("C:\\ProgramData\\PureCVisor\\desktop-node", removeData: true);
        }

        if (action.StartsWith("firewall-", StringComparison.OrdinalIgnoreCase))
        {
            options = options.WithAllowLan();
        }

        if (action == "trust-store-install")
        {
            options = options.WithReleaseApproved().WithTrustStoreCertificateInputs();
        }

        if (action == "trust-store-remove")
        {
            options = options.WithReleaseApproved().WithTrustStoreCertificateInputs(certificatePaths: false);
        }

        return options;
    }
}
