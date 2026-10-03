using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Runtime;

namespace DesktopNode.Api.Tests;

public sealed class ApiReconcileClassificationTests
{
    [Fact]
    public void EveryLedgerMutatingOperationIsExactlyOneReconcileTargetOrClassifiedNonTarget()
    {
        var targets = DesktopNodeJobRuntime.ReconcilableMutations.Keys.ToHashSet(StringComparer.Ordinal);
        var nonTargets = DesktopNodeApiJobReconciliationHandler.ReconcileNonTargets;

        Assert.Empty(targets.Intersect(nonTargets.Keys));
        Assert.All(nonTargets, pair => Assert.False(string.IsNullOrWhiteSpace(pair.Value), pair.Key));

        var unclassified = LedgerMutatingOperations()
            .Where(operation => !targets.Contains(operation) && !nonTargets.ContainsKey(operation))
            .ToArray();
        Assert.Empty(unclassified);
    }

    [Fact]
    public void ReconcileTargetsCoverEveryLedgerOperationWithHostReadback()
    {
        string[] expectedTargets =
        [
            "vm.rename", "vm.delete", "vm.create", "vm.shutdown", "vm.restart", "vm.start", "vm.poweroff",
            "vm.pause", "vm.resume", "vm.save", "vm.resume-saved", "vm.set-memory", "vm.set-vcpu", "vm.disk-resize",
            "checkpoint.create", "checkpoint.restore", "checkpoint.delete", "checkpoint.schedule.set",
            "checkpoint.schedule.clear", "vm.qos.storage.set", "vm.qos.network.set", "console.novnc-target.set",
            "console.novnc-target.clear", "vm.template.lock", "vm.network.connect", "vm.manage", "vm.attach", "vm.eject"
        ];

        Assert.Equal(
            expectedTargets.Order(StringComparer.Ordinal),
            DesktopNodeJobRuntime.ReconcilableMutations.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void NonTargetReconcileExplainsTheClassificationReason()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-non-target-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """
            {
              "version": 1,
              "jobs": [
                {
                  "job_id": "job-guest-exec",
                  "operation": "vm.guest.exec",
                  "status": "failed",
                  "params": { "name": "lab-vm" },
                  "result": null,
                  "error": { "code": "PCV_JOB_INTERRUPTED", "message": "Interrupted.", "detail": "Provider side effect is unresolved.", "retryable": false, "recommended_action": "Reconcile the provider state." },
                  "retry_of": null,
                  "request_id": "req-guest-exec",
                  "correlation_id": "corr-guest-exec",
                  "attempt": 1,
                  "canceled_at": null,
                  "created_at": "2026-09-30T00:00:00.0000000Z",
                  "updated_at": "2026-09-30T00:00:01.0000000Z"
                }
              ],
              "queue": []
            }
            """);
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(jobStorePath: jobStorePath);

            var response = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-guest-exec/reconcile"));

            Assert.Equal(409, response.StatusCode);
            Assert.Contains("job-not-reconcilable", response.Body, StringComparison.Ordinal);
            Assert.Contains("vm.guest.exec is not a reconcile target", response.Body, StringComparison.Ordinal);
            Assert.Contains("confirm whether the mutation applied", response.Body, StringComparison.Ordinal);
            Assert.DoesNotContain("rename", response.Body, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }

    private static IEnumerable<string> LedgerMutatingOperations()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "config", "desktop-node-feature-surface-ledger.json")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory!.FullName, "config", "desktop-node-feature-surface-ledger.json")));
        var operations = new List<string>();
        foreach (var feature in document.RootElement.GetProperty("features").EnumerateArray())
        {
            foreach (var route in feature.GetProperty("routes").EnumerateArray())
            {
                var operation = route.GetProperty("operation_id").GetString()!;
                if (route.GetProperty("method").GetString() != "GET" &&
                    !operation.Contains("preview", StringComparison.Ordinal) &&
                    !operation.StartsWith("auth.", StringComparison.Ordinal))
                {
                    operations.Add(operation);
                }
            }
        }

        Assert.NotEmpty(operations);
        return operations;
    }
}
