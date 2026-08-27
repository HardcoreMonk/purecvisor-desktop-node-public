using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

public sealed partial class ApiRuntimePolicyRequestProcessorTests
{
    private sealed record DesktopNodeHyperVOperationCall(string Operation, string ParamsJson);

    private sealed class RecordingNativeHyperVAdapter : IDesktopNodeHyperVNativeAdapter
    {
        private readonly List<string> calls;
        private readonly Dictionary<string, string> responses;

        public RecordingNativeHyperVAdapter(List<string> calls, string? handledOperation, string? responseJson)
            : this(
                calls,
                handledOperation is null || responseJson is null
                    ? []
                    : new Dictionary<string, string> { [handledOperation] = responseJson })
        {
        }

        public RecordingNativeHyperVAdapter(List<string> calls, Dictionary<string, string> responses)
        {
            this.calls = calls;
            this.responses = responses;
        }

        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(operation);
            if (responses.TryGetValue(operation, out var responseJson))
            {
                result = DesktopNodeHyperVOperationResult.FromJson(responseJson);
                return true;
            }

            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_NATIVE_ROUTE_NOT_HANDLED",
                $"The native adapter did not handle '{operation}'.",
                "No PowerShell helper fallback is available for this product route.",
                false);
            return false;
        }
    }

    private sealed class BlockingGuestExecutionNativeAdapter : IDesktopNodeHyperVNativeAdapter
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource Canceled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            Assert.Equal("vm.guest.exec", operation);
            Started.SetResult();
            cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(10));
            if (cancellationToken.IsCancellationRequested)
            {
                Canceled.SetResult();
                throw new OperationCanceledException(cancellationToken);
            }

            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_TEST_CANCEL_NOT_REQUESTED",
                "The test native adapter was not canceled.",
                "The running cancel test did not request provider cancellation.",
                false);
            return true;
        }
    }

    private sealed class RecordingNativeHyperVMutationAdapter(IList<DesktopNodeHyperVOperationCall> calls) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(new DesktopNodeHyperVOperationCall(operation, parameters.GetRawText()));
            if (operation is "vm.list" or "checkpoint.list")
            {
                result = new DesktopNodeHyperVOperationResult(
                    Ok: true,
                    Operation: operation,
                    Data: JsonSerializer.SerializeToElement(Array.Empty<object>()),
                    Error: null);
                return true;
            }

            var data = new SortedDictionary<string, object?>
            {
                ["name"] = parameters.GetProperty("checkpoint_name").GetString(),
                ["vm_name"] = parameters.GetProperty("vm_name").GetString()
            };
            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(data),
                Error: null);
            return true;
        }
    }

    private sealed class RecordingNativeHyperVPowerStateAdapter(IList<DesktopNodeHyperVOperationCall> calls) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(new DesktopNodeHyperVOperationCall(operation, parameters.GetRawText()));
            var action = operation switch
            {
                "vm.start" => "start",
                "vm.shutdown" => "shutdown",
                "vm.poweroff" => "poweroff",
                "vm.restart" => "restart",
                "vm.pause" => "pause",
                "vm.resume" => "resume",
                "vm.save" => "save",
                "vm.resume-saved" => "resume-saved",
                _ => "unsupported"
            };
            var data = new SortedDictionary<string, object?>
            {
                ["name"] = parameters.GetProperty("name").GetString(),
                ["action"] = action
            };
            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(data),
                Error: null);
            return true;
        }
    }

    private sealed class RecordingNativeHyperVVmRenameAdapter(IList<DesktopNodeHyperVOperationCall> calls) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(new DesktopNodeHyperVOperationCall(operation, parameters.GetRawText()));
            var data = new SortedDictionary<string, object?>
            {
                ["name"] = parameters.GetProperty("name").GetString(),
                ["new_name"] = parameters.GetProperty("new_name").GetString(),
                ["action"] = "rename"
            };
            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(data),
                Error: null);
            return true;
        }
    }

    private sealed class RecordingNativeHyperVVmManageAdapter(IList<DesktopNodeHyperVOperationCall> calls) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(new DesktopNodeHyperVOperationCall(operation, parameters.GetRawText()));
            var data = new SortedDictionary<string, object?>
            {
                ["name"] = parameters.GetProperty("name").GetString(),
                ["action"] = "manage"
            };
            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(data),
                Error: null);
            return true;
        }
    }

    private sealed class RecordingNativeHyperVVmCloneAdapter(IList<DesktopNodeHyperVOperationCall> calls) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(new DesktopNodeHyperVOperationCall(operation, parameters.GetRawText()));
            var source = parameters.GetProperty("source").GetString()!;
            var name = parameters.GetProperty("name").GetString()!;
            if (operation == "vm.clone.preview")
            {
                var data = new SortedDictionary<string, object?>
                {
                    ["action"] = "preview",
                    ["directory"] = $@"D:\PureCVisor\VMs\{name}",
                    ["disk_count"] = 1,
                    ["disks"] = new object[]
                    {
                        new SortedDictionary<string, object?>
                        {
                            ["source"] = $@"D:\PureCVisor\VMs\{source}\disk0.vhdx",
                            ["target"] = $@"D:\PureCVisor\VMs\{name}\disk0.vhdx"
                        }
                    },
                    ["generation"] = 2,
                    ["name"] = name,
                    ["planned_copy_bytes"] = 1024,
                    ["source"] = source
                };
                result = new DesktopNodeHyperVOperationResult(
                    Ok: true,
                    Operation: operation,
                    Data: JsonSerializer.SerializeToElement(data),
                    Error: null);
                return true;
            }

            if (operation == "vm.clone")
            {
                var data = new SortedDictionary<string, object?>
                {
                    ["action"] = "clone",
                    ["directory"] = $@"D:\PureCVisor\VMs\{name}",
                    ["disks"] = new[] { $@"D:\PureCVisor\VMs\{name}\disk0.vhdx" },
                    ["name"] = name,
                    ["source"] = source
                };
                result = new DesktopNodeHyperVOperationResult(
                    Ok: true,
                    Operation: operation,
                    Data: JsonSerializer.SerializeToElement(data),
                    Error: null);
                return true;
            }

            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_NATIVE_ROUTE_NOT_HANDLED",
                $"The native adapter did not handle '{operation}'.",
                "No PowerShell helper fallback is available for this product route.",
                false);
            return false;
        }
    }

    private sealed class RecordingNativeHyperVVmMediaAdapter(IList<DesktopNodeHyperVOperationCall> calls) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(new DesktopNodeHyperVOperationCall(operation, parameters.GetRawText()));
            var data = new SortedDictionary<string, object?>
            {
                ["name"] = parameters.GetProperty("name").GetString(),
                ["action"] = operation == "vm.attach" ? "attach" : "eject"
            };
            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(data),
                Error: null);
            return true;
        }
    }

    private sealed class RecordingNativeHyperVVmResourceMutationAdapter(IList<DesktopNodeHyperVOperationCall> calls) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(new DesktopNodeHyperVOperationCall(operation, parameters.GetRawText()));
            var action = operation switch
            {
                "vm.set-memory" => "set-memory",
                "vm.set-vcpu" => "set-vcpu",
                "vm.disk-resize" => "disk-resize",
                "vm.limit" => "limit",
                _ => "unsupported"
            };
            var data = new SortedDictionary<string, object?>
            {
                ["name"] = parameters.GetProperty("name").GetString(),
                ["action"] = action
            };
            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(data),
                Error: null);
            return true;
        }
    }

    private sealed class RecordingNativeHyperVQosMutationAdapter(IList<DesktopNodeHyperVOperationCall> calls) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(new DesktopNodeHyperVOperationCall(operation, parameters.GetRawText()));
            var vmName = parameters.GetProperty("name").GetString()!;
            var target = parameters.TryGetProperty("disk", out var disk)
                ? disk.GetString()!
                : parameters.GetProperty("adapter").GetString()!;

            if (operation.EndsWith(".preview", StringComparison.Ordinal))
            {
                var section = operation.Contains(".storage.", StringComparison.Ordinal) ? "storage" : "network";
                var targetProperty = section == "storage" ? "target_disk" : "adapter";
                var policyProperty = section == "storage" ? "maximum_iops" : "maximum_kbps";
                var value = section == "storage"
                    ? parameters.GetProperty("maximum_iops").GetInt32()
                    : parameters.GetProperty("maximum_kbps").GetInt32();
                var data = new SortedDictionary<string, object?>
                {
                    ["contract"] = "hyperv-qos-mutation-preview.v1",
                    ["mode"] = "dry-run",
                    ["provider"] = "hyperv",
                    ["request_id"] = "req-qos-preview",
                    ["actor"] = "local-api-operator",
                    ["vm"] = new SortedDictionary<string, object?> { ["name"] = vmName },
                    [section] = new SortedDictionary<string, object?>
                    {
                        [targetProperty] = target,
                        ["proposed_policy"] = new SortedDictionary<string, object?> { [policyProperty] = value },
                        ["supported"] = true
                    },
                    ["validation"] = new SortedDictionary<string, object?>
                    {
                        ["requires_admin"] = true,
                        ["host_mutation_performed"] = false
                    }
                };
                result = new DesktopNodeHyperVOperationResult(true, operation, JsonSerializer.SerializeToElement(data), null);
                return true;
            }

            var action = operation.Contains(".storage.", StringComparison.Ordinal) ? "storage-qos" : "network-qos";
            var rollbackOperation = operation.Contains(".storage.", StringComparison.Ordinal)
                ? "vm.qos.storage.rollback"
                : "vm.qos.network.rollback";
            var evidence = new SortedDictionary<string, object?>
            {
                ["contract"] = "hyperv-qos-mutation-apply-evidence.v1",
                ["operation"] = operation,
                ["vm"] = new SortedDictionary<string, object?> { ["name"] = vmName },
                ["target"] = target,
                ["previous_policy"] = "unset",
                ["applied_policy"] = "test-policy",
                ["rollback_plan"] = new SortedDictionary<string, object?> { ["rollback_operation"] = rollbackOperation },
                ["audit"] = new SortedDictionary<string, object?> { ["args_redacted"] = true }
            };
            var payload = new SortedDictionary<string, object?>
            {
                ["name"] = vmName,
                ["action"] = action,
                ["evidence"] = evidence
            };
            result = new DesktopNodeHyperVOperationResult(true, operation, JsonSerializer.SerializeToElement(payload), null);
            return true;
        }
    }

    private sealed class RecordingNativeHyperVVmInventoryReadbackAdapter(IList<DesktopNodeHyperVOperationCall> calls) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(new DesktopNodeHyperVOperationCall(operation, parameters.GetRawText()));
            var vmName = parameters.GetProperty("vm_name").GetString();
            var data = operation switch
            {
                "vm.memory-stats" => new SortedDictionary<string, object?>
                {
                    ["name"] = vmName,
                    ["memory"] = new SortedDictionary<string, object?>
                    {
                        ["startup_mb"] = 4096,
                        ["assigned_mb"] = 2048,
                        ["dynamic"] = false
                    },
                    ["state"] = "running"
                },
                "vm.cpu-stats" => new SortedDictionary<string, object?>
                {
                    ["name"] = vmName,
                    ["cpu"] = new SortedDictionary<string, object?>
                    {
                        ["count"] = 2
                    },
                    ["state"] = "running"
                },
                "vm.blkio-get" => new SortedDictionary<string, object?>
                {
                    ["name"] = vmName,
                    ["state"] = "running",
                    ["storage_qos"] = new SortedDictionary<string, object?>
                    {
                        ["linux_blkio_compatible"] = false
                    }
                },
                "vm.bandwidth" => new SortedDictionary<string, object?>
                {
                    ["name"] = vmName,
                    ["state"] = "running",
                    ["network_qos"] = new SortedDictionary<string, object?>
                    {
                        ["linux_bandwidth_compatible"] = false
                    }
                },
                "vm.guest-agent-status" => new SortedDictionary<string, object?>
                {
                    ["name"] = vmName,
                    ["state"] = "running",
                    ["guest_agent"] = new SortedDictionary<string, object?>
                    {
                        ["qemu_guest_agent"] = false
                    }
                },
                _ => new SortedDictionary<string, object?>
                {
                    ["name"] = vmName,
                    ["state"] = "running",
                    ["guest_ping"] = new SortedDictionary<string, object?>
                    {
                        ["guest_heartbeat_verified"] = false
                    }
                }
            };

            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(data),
                Error: null);
            return true;
        }
    }

    private sealed class RecordingNativeHyperVCreateAdapter(IList<DesktopNodeHyperVOperationCall> calls) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(new DesktopNodeHyperVOperationCall(operation, parameters.GetRawText()));
            var name = parameters.GetProperty("name").GetString()!;
            var vmRoot = parameters.GetProperty("vm_root").GetString()!;
            var data = new SortedDictionary<string, object?>
            {
                ["name"] = name,
                ["vm_dir"] = Path.Combine(vmRoot, name),
                ["vhd_path"] = Path.Combine(vmRoot, name, "disk0.vhdx"),
                ["iso_path"] = parameters.GetProperty("iso_path").GetString(),
                ["switch"] = "Default Switch",
                ["generation"] = parameters.GetProperty("generation").GetInt32(),
                ["steps"] = new[] { "Create VM folder", "Create VHDX", "Create Hyper-V VM" }
            };
            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(data),
                Error: null);
            return true;
        }
    }

    private sealed class RecordingNativeHyperVVmDeleteAdapter(IList<DesktopNodeHyperVOperationCall> calls) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(new DesktopNodeHyperVOperationCall(operation, parameters.GetRawText()));
            var data = new SortedDictionary<string, object?>
            {
                ["name"] = parameters.GetProperty("name").GetString(),
                ["action"] = "delete"
            };
            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(data),
                Error: null);
            return true;
        }
    }

    private sealed class RecordingHyperVSwitchProvider(IReadOnlyList<DesktopNodeHyperVSwitchInfo> switches) : IDesktopNodeHyperVSwitchProvider
    {
        public IReadOnlyList<DesktopNodeHyperVSwitchInfo> GetSwitches(CancellationToken cancellationToken)
        {
            return switches;
        }
    }

    private sealed class RecordingHyperVVmProvider(IReadOnlyList<DesktopNodeHyperVVmInfo> vms) : IDesktopNodeHyperVVmProvider
    {
        public IReadOnlyList<DesktopNodeHyperVVmInfo> GetVms(CancellationToken cancellationToken)
        {
            return vms;
        }
    }

    private sealed class RecordingHyperVCheckpointProvider(IReadOnlyList<DesktopNodeHyperVCheckpointInfo> checkpoints) : IDesktopNodeHyperVCheckpointProvider
    {
        public IReadOnlyList<DesktopNodeHyperVCheckpointInfo> GetCheckpoints(string vmName, CancellationToken cancellationToken)
        {
            return checkpoints;
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(directory))
        {
            if (File.Exists(Path.Combine(directory, "src", "DesktopNode.sln")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    private static string ToRepoRootRedactedPath(string path, string repoRoot)
    {
        var relative = Path.GetRelativePath(repoRoot, path);
        return Path.Combine("[REPO_ROOT]", relative);
    }

    private sealed class BlockingConcurrencyNativeHyperVAdapter : IDesktopNodeHyperVNativeAdapter
    {
        private readonly ManualResetEventSlim firstCallEntered = new(false);
        private readonly ManualResetEventSlim concurrentCallEntered = new(false);
        private readonly ManualResetEventSlim releaseFirstCall = new(false);
        private int activeCalls;
        private int callCount;
        private int maxConcurrent;

        public int CallCount => Volatile.Read(ref callCount);

        public int MaxConcurrent => Volatile.Read(ref maxConcurrent);

        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            var active = Interlocked.Increment(ref activeCalls);
            RecordMaxConcurrent(active);
            if (active > 1)
            {
                concurrentCallEntered.Set();
            }

            var callNumber = Interlocked.Increment(ref callCount);
            try
            {
                if (callNumber == 1)
                {
                    firstCallEntered.Set();
                    if (!releaseFirstCall.Wait(TimeSpan.FromSeconds(5)))
                    {
                        throw new TimeoutException("The first helper call was not released.");
                    }
                }

                result = DesktopNodeHyperVOperationResult.FromJson("""
                {"ok":true,"operation":"host.status","data":{"marker":"serialized"},"error":null}
                """);
                return true;
            }
            finally
            {
                Interlocked.Decrement(ref activeCalls);
            }
        }

        public bool WaitForFirstCall(TimeSpan timeout)
        {
            return firstCallEntered.Wait(timeout);
        }

        public bool WaitForConcurrentCall(TimeSpan timeout)
        {
            return concurrentCallEntered.Wait(timeout);
        }

        public void ReleaseFirstCall()
        {
            releaseFirstCall.Set();
        }

        private void RecordMaxConcurrent(int active)
        {
            while (true)
            {
                var snapshot = Volatile.Read(ref maxConcurrent);
                if (active <= snapshot)
                {
                    return;
                }

                if (Interlocked.CompareExchange(ref maxConcurrent, active, snapshot) == snapshot)
                {
                    return;
                }
            }
        }
    }
}
