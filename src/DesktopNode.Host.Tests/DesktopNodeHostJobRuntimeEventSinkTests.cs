using System.Text.Json;
using System.Runtime.InteropServices;
using DesktopNode.Runtime;

namespace DesktopNode.Host.Tests;

public sealed class DesktopNodeHostJobRuntimeEventSinkTests
{
    [Theory]
    [InlineData("")]
    [InlineData(".commit-pending")]
    [InlineData(".tmp")]
    [InlineData(".tmp.0123456789abcdef0123456789abcdef")]
    [InlineData(".commit-pending.tmp.0123456789abcdef0123456789abcdef")]
    public void HostStartupRejectsJsonLinePathOverlappingManagedJobStoreFiles(string suffix)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pcv-host-job-runtime-path-conflict-" + Guid.NewGuid().ToString("N"));
        var storePath = Path.Combine(root, "jobs.json");
        var options = new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/",
            JobStorePath = storePath,
            EventLogPath = storePath + suffix,
            EventLogWriter = "jsonl"
        };

        var exception = Assert.Throws<ArgumentException>(() =>
        {
            _ = DesktopNodeHostApplication.StartAsync(options);
        });

        Assert.Contains("PCV_HOST_EVENT_LOG_JOB_STORE_CONFLICT", exception.Message, StringComparison.Ordinal);
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public void HostStartupRejectsRotationTargetOverlappingJobStore()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pcv-host-job-runtime-rotation-conflict-" + Guid.NewGuid().ToString("N"));
        var eventPath = Path.Combine(root, "events.jsonl");
        var options = new DesktopNodeHostOptions
        {
            Prefix = "http://127.0.0.1:0/",
            JobStorePath = eventPath + ".1",
            EventLogPath = eventPath,
            EventLogWriter = "jsonl"
        };

        var exception = Assert.Throws<ArgumentException>(() =>
        {
            _ = DesktopNodeHostApplication.StartAsync(options);
        });

        Assert.Contains("PCV_HOST_EVENT_LOG_JOB_STORE_CONFLICT", exception.Message, StringComparison.Ordinal);
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public void HostStartupRejectsAlternateDataStreamEventAlias()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pcv-host-job-runtime-ads-conflict-" + Guid.NewGuid().ToString("N"));
        var storePath = Path.Combine(root, "jobs.json");
        var options = new DesktopNodeHostOptions
        {
            Prefix = "http://127.0.0.1:0/",
            JobStorePath = storePath,
            EventLogPath = storePath + "::$DATA",
            EventLogWriter = "jsonl"
        };

        var exception = Assert.Throws<ArgumentException>(() =>
        {
            _ = DesktopNodeHostApplication.StartAsync(options);
        });

        Assert.Contains("alternate data stream", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(Directory.Exists(root));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HostStartupRejectsEventAndJobStoreAncestorOverlap(bool eventIsDescendant)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pcv-host-job-runtime-containment-conflict-" + Guid.NewGuid().ToString("N"));
        var anchor = Path.Combine(root, "state");
        var storePath = eventIsDescendant
            ? anchor
            : Path.Combine(anchor, "jobs.json");
        var eventPath = eventIsDescendant
            ? Path.Combine(anchor, "events.jsonl")
            : anchor;
        var options = new DesktopNodeHostOptions
        {
            Prefix = "http://127.0.0.1:0/",
            JobStorePath = storePath,
            EventLogPath = eventPath,
            EventLogWriter = "jsonl"
        };

        var exception = Assert.Throws<ArgumentException>(() =>
        {
            _ = DesktopNodeHostApplication.StartAsync(options);
        });

        Assert.Contains("PCV_HOST_EVENT_LOG_JOB_STORE_CONFLICT", exception.Message, StringComparison.Ordinal);
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public void HostStartupRejectsExistingHardLinkAliasBetweenEventAndJobStore()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var root = Path.Combine(
            Path.GetTempPath(),
            "pcv-host-job-runtime-hardlink-conflict-" + Guid.NewGuid().ToString("N"));
        var storePath = Path.Combine(root, "jobs.json");
        var eventPath = Path.Combine(root, "events.jsonl");
        Directory.CreateDirectory(root);
        File.WriteAllText(storePath, """{"version":1,"jobs":[],"queue":[]}""");
        try
        {
            if (!CreateHardLink(eventPath, storePath, IntPtr.Zero))
            {
                return;
            }

            var exception = Assert.Throws<ArgumentException>(() =>
                DesktopNodeHostJobRuntimeEventSink.ValidatePathSeparation(
                    new DesktopNodeHostOptions
                    {
                        JobStorePath = storePath,
                        EventLogPath = eventPath
                    }));

            Assert.Contains("PCV_HOST_EVENT_LOG_JOB_STORE_CONFLICT", exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public async Task HostStartupRoutesBlockedStoreObservationToConfiguredJsonLineSink()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pcv-host-job-runtime-wiring-" + Guid.NewGuid().ToString("N"));
        var storePath = Path.Combine(root, "jobs.json");
        var eventPath = Path.Combine(root, "events.jsonl");
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(storePath, """{"version":99,"jobs":[],"queue":[]}""");

            using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
            {
                Mode = DesktopNodeHostMode.Listen,
                Prefix = "http://127.0.0.1:0/",
                JobStorePath = storePath,
                EventLogPath = eventPath,
                EventLogWriter = "jsonl"
            });

            string[] lines = [];
            Assert.True(WaitUntil(() => TryReadNonEmptyLines(eventPath, out lines)));
            var line = Assert.Single(lines);
            using var document = JsonDocument.Parse(line);
            Assert.Equal("load-blocked", document.RootElement.GetProperty("event").GetString());
            Assert.Equal(
                "PCV_JOB_STORE_SCHEMA_UNSUPPORTED",
                document.RootElement.GetProperty("code").GetString());
            Assert.DoesNotContain(storePath, line, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void JsonLineSinkWritesOnlyTheRedactedObservationContract()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pcv-host-job-runtime-event-" + Guid.NewGuid().ToString("N"));
        var path = Path.Combine(root, "events.jsonl");
        try
        {
            var sink = new DesktopNodeHostJobRuntimeEventSink(new DesktopNodeHostOptions
            {
                EventLogPath = path,
                EventLogWriter = "jsonl"
            });
            sink.Write(new DesktopNodeJobRuntimeObservation(
                "save-indeterminate",
                "PCV_JOB_STORE_SAVE_FAILED",
                "indeterminate",
                "2026-08-02T00:00:00.0000000+00:00",
                "Preserve the pending guard."));

            var line = Assert.Single(File.ReadAllLines(path));
            using var document = JsonDocument.Parse(line);
            var rootElement = document.RootElement;
            Assert.Equal(
                ["code", "commit_outcome", "event", "occurred_at", "recommended_action"],
                rootElement.EnumerateObject().Select(property => property.Name).Order().ToArray());
            Assert.Equal("save-indeterminate", rootElement.GetProperty("event").GetString());
            Assert.DoesNotContain("jobs.json", line, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("params", line, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("exception", line, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void UnwritableJsonLineSinkDoesNotThrow()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pcv-host-job-runtime-event-file-" + Guid.NewGuid().ToString("N"));
        try
        {
            File.WriteAllText(root, "directory-blocker");
            var sink = new DesktopNodeHostJobRuntimeEventSink(new DesktopNodeHostOptions
            {
                EventLogPath = Path.Combine(root, "events.jsonl"),
                EventLogWriter = "jsonl"
            });

            var exception = Record.Exception(() => sink.Write(new DesktopNodeJobRuntimeObservation(
                "load-blocked",
                "PCV_JOB_STORE_CORRUPT",
                null,
                "2026-08-02T00:00:00.0000000+00:00",
                "Preserve the store.")));

            Assert.Null(exception);
        }
        finally
        {
            if (File.Exists(root))
            {
                File.Delete(root);
            }
        }
    }

    [Fact]
    public void JsonLineSinkRotatesAtBoundedSizeAndRetainsConfiguredHistory()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pcv-host-job-runtime-rotation-" + Guid.NewGuid().ToString("N"));
        var path = Path.Combine(root, "events.jsonl");
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(path, new string('c', 180));
            File.WriteAllText(path + ".1", "previous-one");
            File.WriteAllText(path + ".2", "previous-two");
            var sink = new DesktopNodeHostJobRuntimeEventSink(
                new DesktopNodeHostOptions
                {
                    EventLogPath = path,
                    EventLogWriter = "jsonl"
                },
                (_, _, _) => false,
                (_, _, _, _) => { },
                maxJsonLineBytes: 200,
                retainedJsonLineFiles: 2,
                writeWindowsEventLogOverride: false);

            sink.Write(new DesktopNodeJobRuntimeObservation(
                "save-not-committed",
                "PCV_JOB_STORE_SAVE_FAILED",
                "notcommitted",
                "2026-08-02T00:00:00.0000000+00:00",
                "Restore write access."));

            Assert.Equal(new string('c', 180), File.ReadAllText(path + ".1"));
            Assert.Equal("previous-one", File.ReadAllText(path + ".2"));
            var line = Assert.Single(File.ReadAllLines(path));
            using var document = JsonDocument.Parse(line);
            Assert.Equal("save-not-committed", document.RootElement.GetProperty("event").GetString());
            Assert.True(new FileInfo(path).Length <= 200);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void MissingWindowsEventSourceUsesJsonFallbackWithoutCallingWriteEntry()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pcv-host-job-runtime-source-" + Guid.NewGuid().ToString("N"));
        var path = Path.Combine(root, "events.jsonl");
        var sourceChecks = 0;
        var eventWrites = 0;
        try
        {
            var sink = new DesktopNodeHostJobRuntimeEventSink(
                new DesktopNodeHostOptions
                {
                    EventLogPath = path,
                    EventLogProviderSource = "PureCVisor Missing Test Source",
                    EventLogWriter = "windows-event-log"
                },
                (logName, sourceName, expectedEventMessageFile) =>
                {
                    sourceChecks++;
                    Assert.Equal("Application", logName);
                    Assert.Equal("PureCVisor Missing Test Source", sourceName);
                    Assert.False(string.IsNullOrWhiteSpace(expectedEventMessageFile));
                    return false;
                },
                (_, _, _, _) => eventWrites++,
                DesktopNodeHostJobRuntimeEventSink.DefaultMaxJsonLineBytes,
                DesktopNodeHostJobRuntimeEventSink.DefaultRetainedJsonLineFiles,
                writeWindowsEventLogOverride: true);

            sink.Write(new DesktopNodeJobRuntimeObservation(
                "load-blocked",
                "PCV_JOB_STORE_CORRUPT",
                null,
                "2026-08-02T00:00:00.0000000+00:00",
                "Preserve the store."));

            Assert.Equal(1, sourceChecks);
            Assert.Equal(0, eventWrites);
            Assert.Single(File.ReadAllLines(path));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void EventMessageFileUsesTheCurrentExecutableWhenAvailable()
    {
        var processPath = Path.Combine(Path.GetTempPath(), "pcv-current-host.exe");

        var resolved = DesktopNodeHostJobRuntimeEventSink.ResolveEventMessageFile(
            processPath,
            Path.Combine(Path.GetTempPath(), "ignored-base"));

        Assert.Equal(Path.GetFullPath(processPath), resolved);
    }

    [Fact]
    public void EventMessageFileFallsBackToTheSingleFileHostInTheApplicationDirectory()
    {
        var baseDirectory = Path.Combine(Path.GetTempPath(), "pcv-single-file-host");

        var resolved = DesktopNodeHostJobRuntimeEventSink.ResolveEventMessageFile(null, baseDirectory);

        Assert.Equal(
            Path.GetFullPath(Path.Combine(baseDirectory, "DesktopNode.Host.exe")),
            resolved);
    }

    private static bool TryReadNonEmptyLines(string path, out string[] lines)
    {
        lines = [];
        try
        {
            if (!File.Exists(path))
            {
                return false;
            }

            lines = File.ReadAllLines(path);
            return lines.Length > 0;
        }
        catch (IOException)
        {
            return false;
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateHardLink(
        string lpFileName,
        string lpExistingFileName,
        IntPtr lpSecurityAttributes);

    // Waits by sleeping rather than spinning. SpinWait burns CPU that the work being waited
    // on needs, so on a loaded runner a short deadline can expire while the work is merely
    // starved. The timeout here is a hang guard, not a performance threshold.
    private static bool WaitUntil(Func<bool> condition)
    {
        var deadline = Environment.TickCount64 + (long)TimeSpan.FromSeconds(60).TotalMilliseconds;
        while (!condition())
        {
            if (Environment.TickCount64 >= deadline)
            {
                return false;
            }

            Thread.Sleep(15);
        }

        return true;
    }

}
