namespace DesktopNode.Api;

// reconcile 비대상 mutation 과 그 이유다. 대상 집합은 Runtime DesktopNodeJobRuntime.ReconcilableMutations 가 소유한다.
// surface ledger 의 모든 mutating operation 은 둘 중 정확히 한 곳에 있어야 한다(ApiReconcileClassificationTests).
internal sealed partial class DesktopNodeApiJobReconciliationHandler
{
    internal static readonly IReadOnlyDictionary<string, string> ReconcileNonTargets = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["job.cancel"] = "Job control route; it changes the job record and queues no provider mutation.",
        ["job.retry"] = "Job control route; it queues a new attempt that owns its own reconciliation.",
        ["job.reconcile"] = "Job control route; it is the reconciliation itself.",
        ["diagnostic.bundle.create"] = "Diagnostic bundles are local evidence files; a failed bundle is recreated, not reconciled.",
        ["account.create"] = "Account store writes are synchronous and report their own outcome.",
        ["account.disable"] = "Account store writes are synchronous and report their own outcome.",
        ["vm.attach"] = "vm.list exposes no DVD media path, so the attached ISO cannot be read back.",
        ["vm.eject"] = "vm.list exposes no DVD media path, so the ejected ISO cannot be read back.",
        ["vm.device.add"] = "A repeated add creates another NIC or DVD drive, and vm.list cannot tell which add applied.",
        ["vm.nic.add"] = "A repeated add creates another NIC, and vm.list cannot tell which add applied.",
        ["vm.dvd.add"] = "A repeated add creates another DVD drive, and vm.list cannot tell which add applied.",
        ["vm.guest.exec"] = "Guest command side effects are not observable from the host and repeating them is unsafe.",
        ["vm.guest.file"] = "Guest file transfer outcomes are not observable from vm.list; verify the guest path and resubmit.",
        ["vm.guest.channel.verify"] = "Verification is a probe without a host-side postcondition; submit a new probe.",
        ["vm.guest.channel.ensure"] = "The guest-side channel state has no host readback.",
        ["vm.clone"] = "A partial clone leaves copied disks under a new VM; use inventory and the managed delete path.",
        ["vm.import"] = "A partial import registers or copies a VM from an export directory; use inventory and the managed delete path.",
        ["vm.export"] = "Export writes host files that vm.list does not report.",
        ["vm.limit"] = "Combined limit command; the per-field routes (set-memory, set-vcpu, QoS) reconcile each value.",
    };
}
