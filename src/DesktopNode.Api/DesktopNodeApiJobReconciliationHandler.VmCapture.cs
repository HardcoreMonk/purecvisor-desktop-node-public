using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

internal sealed partial class DesktopNodeApiJobReconciliationHandler
{
    private JsonElement CaptureVmCreateBaseline(
        string vmName,
        int generation,
        CancellationToken cancellationToken)
    {
        var expectedBefore = new SortedDictionary<string, object?>
        {
            ["state"] = "absent",
            ["name"] = vmName
        };
        var expectedAfter = new SortedDictionary<string, object?>
        {
            ["state"] = "present",
            ["name"] = vmName,
            ["generation"] = generation,
            ["managed_by_purecvisor"] = true
        };

        if (string.IsNullOrWhiteSpace(vmName))
        {
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmCreateReconciliationSchema,
                ["capture_status"] = "unavailable",
                ["capture_error_code"] = "PCV_VM_NAME_REQUIRED",
                ["before"] = null,
                ["expected_before"] = expectedBefore,
                ["expected_after"] = expectedAfter
            });
        }

        try
        {
            using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
            var readback = operationInvoker.Invoke("vm.list", DesktopNodeApiResponseFactory.EmptyObject(), readbackTimeout.Token);
            if (!readback.Ok || readback.Data is null)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmCreateReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = readback.Error?.Code ?? "PCV_VM_LIST_FAILED",
                    ["before"] = null,
                    ["expected_before"] = expectedBefore,
                    ["expected_after"] = expectedAfter
                });
            }

            var matching = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
                .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), vmName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (matching.Length != 0)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmCreateReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = matching.Length == 1
                        ? "PCV_VM_ALREADY_EXISTS"
                        : "PCV_VM_IDENTITY_AMBIGUOUS",
                    ["before"] = matching.Length == 1 ? matching[0] : null,
                    ["expected_before"] = expectedBefore,
                    ["expected_after"] = expectedAfter
                });
            }

            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmCreateReconciliationSchema,
                ["capture_status"] = "captured",
                ["before"] = null,
                ["expected_before"] = expectedBefore,
                ["expected_after"] = expectedAfter
            });
        }
        catch (Exception)
        {
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmCreateReconciliationSchema,
                ["capture_status"] = "unavailable",
                ["capture_error_code"] = "PCV_VM_LIST_FAILED",
                ["before"] = null,
                ["expected_before"] = expectedBefore,
                ["expected_after"] = expectedAfter
            });
        }
    }

    private JsonElement CaptureVmShutdownBaseline(string vmName, CancellationToken cancellationToken)
    {
        var expectedAfter = new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            ["state"] = "off"
        };

        try
        {
            using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
            var readback = operationInvoker.Invoke("vm.list", DesktopNodeApiResponseFactory.EmptyObject(), readbackTimeout.Token);
            if (!readback.Ok || readback.Data is null)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmShutdownReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = readback.Error?.Code ?? "PCV_VM_LIST_FAILED",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = expectedAfter
                });
            }

            var matches = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
                .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), vmName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (matches.Length != 1)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmShutdownReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = matches.Length == 0 ? "PCV_VM_NOT_FOUND" : "PCV_VM_IDENTITY_AMBIGUOUS",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = expectedAfter
                });
            }

            var before = matches[0].Clone();
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmShutdownReconciliationSchema,
                ["capture_status"] = "captured",
                ["before"] = before,
                ["before_fingerprint"] = BuildVmShutdownIdentityFingerprint(before),
                ["expected_after"] = expectedAfter
            });
        }
        catch (Exception)
        {
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmShutdownReconciliationSchema,
                ["capture_status"] = "unavailable",
                ["capture_error_code"] = "PCV_VM_LIST_FAILED",
                ["before"] = null,
                ["before_fingerprint"] = null,
                ["expected_after"] = expectedAfter
            });
        }
    }

    private JsonElement CaptureVmRestartBaseline(string vmName, CancellationToken cancellationToken)
    {
        var expectedAfter = new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            ["state"] = "running"
        };

        try
        {
            using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
            var readback = operationInvoker.Invoke("vm.list", DesktopNodeApiResponseFactory.EmptyObject(), readbackTimeout.Token);
            if (!readback.Ok || readback.Data is null)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmRestartReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = readback.Error?.Code ?? "PCV_VM_LIST_FAILED",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = expectedAfter
                });
            }

            var matches = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
                .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), vmName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (matches.Length != 1)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmRestartReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = matches.Length == 0 ? "PCV_VM_NOT_FOUND" : "PCV_VM_IDENTITY_AMBIGUOUS",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = expectedAfter
                });
            }

            var before = matches[0].Clone();
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmRestartReconciliationSchema,
                ["capture_status"] = "captured",
                ["before"] = before,
                ["before_fingerprint"] = BuildVmShutdownIdentityFingerprint(before),
                ["expected_after"] = expectedAfter
            });
        }
        catch (Exception)
        {
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmRestartReconciliationSchema,
                ["capture_status"] = "unavailable",
                ["capture_error_code"] = "PCV_VM_LIST_FAILED",
                ["before"] = null,
                ["before_fingerprint"] = null,
                ["expected_after"] = expectedAfter
            });
        }
    }

    private JsonElement CaptureVmRenameBaseline(
        string oldName,
        string newName,
        CancellationToken cancellationToken)
    {
        try
        {
            using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
            var readback = operationInvoker.Invoke("vm.list", DesktopNodeApiResponseFactory.EmptyObject(), readbackTimeout.Token);
            if (!readback.Ok || readback.Data is null)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmRenameReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = readback.Error?.Code ?? "PCV_VM_LIST_FAILED",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = new SortedDictionary<string, object?> { ["name"] = newName }
                });
            }

            var matches = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
                .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), oldName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (matches.Length != 1)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmRenameReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = matches.Length == 0 ? "PCV_VM_NOT_FOUND" : "PCV_VM_IDENTITY_AMBIGUOUS",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = new SortedDictionary<string, object?> { ["name"] = newName }
                });
            }

            var before = matches[0].Clone();
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmRenameReconciliationSchema,
                ["capture_status"] = "captured",
                ["before"] = before,
                ["before_fingerprint"] = BuildVmRenameFingerprint(before),
                ["expected_after"] = new SortedDictionary<string, object?> { ["name"] = newName }
            });
        }
        catch (Exception)
        {
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmRenameReconciliationSchema,
                ["capture_status"] = "unavailable",
                ["capture_error_code"] = "PCV_VM_LIST_FAILED",
                ["before"] = null,
                ["before_fingerprint"] = null,
                ["expected_after"] = new SortedDictionary<string, object?> { ["name"] = newName }
            });
        }
    }

    private JsonElement CaptureVmDeleteBaseline(
        string vmName,
        CancellationToken cancellationToken)
    {
        try
        {
            using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
            var readback = operationInvoker.Invoke("vm.list", DesktopNodeApiResponseFactory.EmptyObject(), readbackTimeout.Token);
            if (!readback.Ok || readback.Data is null)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmDeleteReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = readback.Error?.Code ?? "PCV_VM_LIST_FAILED",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = new SortedDictionary<string, object?>
                    {
                        ["name"] = vmName,
                        ["state"] = "absent"
                    }
                });
            }

            var matches = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
                .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), vmName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (matches.Length != 1)
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmDeleteReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = matches.Length == 0 ? "PCV_VM_NOT_FOUND" : "PCV_VM_IDENTITY_AMBIGUOUS",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = new SortedDictionary<string, object?>
                    {
                        ["name"] = vmName,
                        ["state"] = "absent"
                    }
                });
            }

            var before = matches[0].Clone();
            if (!IsManagedVm(before))
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmDeleteReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = "PCV_VM_NOT_MANAGED_BY_PURECVISOR",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = new SortedDictionary<string, object?>
                    {
                        ["name"] = vmName,
                        ["state"] = "absent"
                    }
                });
            }

            if (string.IsNullOrWhiteSpace(DesktopNodeApiJsonReader.ReadString(before, "id")))
            {
                return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["schema"] = VmDeleteReconciliationSchema,
                    ["capture_status"] = "unavailable",
                    ["capture_error_code"] = "PCV_VM_IDENTITY_UNAVAILABLE",
                    ["before"] = null,
                    ["before_fingerprint"] = null,
                    ["expected_after"] = new SortedDictionary<string, object?>
                    {
                        ["name"] = vmName,
                        ["state"] = "absent"
                    }
                });
            }

            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmDeleteReconciliationSchema,
                ["capture_status"] = "captured",
                ["before"] = before,
                ["before_fingerprint"] = BuildVmDeleteFingerprint(before),
                ["expected_after"] = new SortedDictionary<string, object?>
                {
                    ["name"] = vmName,
                    ["state"] = "absent"
                }
            });
        }
        catch (Exception)
        {
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmDeleteReconciliationSchema,
                ["capture_status"] = "unavailable",
                ["capture_error_code"] = "PCV_VM_LIST_FAILED",
                ["before"] = null,
                ["before_fingerprint"] = null,
                ["expected_after"] = new SortedDictionary<string, object?>
                {
                    ["name"] = vmName,
                    ["state"] = "absent"
                }
            });
        }
    }
}
