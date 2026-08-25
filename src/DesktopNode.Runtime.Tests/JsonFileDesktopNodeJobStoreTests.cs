using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using DesktopNode.Runtime;

namespace DesktopNode.Runtime.Tests;

public sealed class JsonFileDesktopNodeJobStoreTests
{
    [Fact]
    public void WriterFlushesUniqueTempAndPendingGuardBeforeReplacingPrimary()
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        var candidate = EmptyStoreJson(version: 1);
        var stages = new List<JsonFileDesktopNodeJobStoreWriteStage>();
        string? candidateTempPath = null;
        string? pendingTempPath = null;
        var tempMatchedCandidate = false;
        var pendingGuardExisted = false;
        var primaryMatchedCandidate = false;
        Directory.CreateDirectory(root);

        try
        {
            var store = new JsonFileDesktopNodeJobStore(
                storePath,
                (stage, tempPath) =>
                {
                    stages.Add(stage);
                    if (stage == JsonFileDesktopNodeJobStoreWriteStage.CandidateTempFlushed)
                    {
                        candidateTempPath = tempPath;
                        tempMatchedCandidate = File.ReadAllText(tempPath) == candidate;
                    }
                    else if (stage == JsonFileDesktopNodeJobStoreWriteStage.PendingCommitTempFlushed)
                    {
                        pendingTempPath = tempPath;
                    }
                    else if (stage == JsonFileDesktopNodeJobStoreWriteStage.PendingCommitFlushed)
                    {
                        pendingGuardExisted = File.Exists(storePath + ".commit-pending");
                    }
                    else if (stage == JsonFileDesktopNodeJobStoreWriteStage.PrimaryReplaced)
                    {
                        primaryMatchedCandidate = File.ReadAllText(storePath) == candidate;
                    }
                });

            var result = store.WriteSnapshot(candidate);

            Assert.Equal(DesktopNodeJobStoreCommitOutcome.Committed, result.Outcome);
            Assert.Null(result.Failure);
            Assert.Equal(
                [
                    JsonFileDesktopNodeJobStoreWriteStage.CandidateTempFlushed,
                    JsonFileDesktopNodeJobStoreWriteStage.PendingCommitTempFlushed,
                    JsonFileDesktopNodeJobStoreWriteStage.PendingCommitFlushed,
                    JsonFileDesktopNodeJobStoreWriteStage.PrimaryReplaced
                ],
                stages);
            Assert.NotNull(candidateTempPath);
            Assert.NotNull(pendingTempPath);
            Assert.Equal(root, Path.GetDirectoryName(candidateTempPath));
            Assert.Equal(root, Path.GetDirectoryName(pendingTempPath));
            Assert.StartsWith(storePath + ".tmp.", candidateTempPath, StringComparison.Ordinal);
            Assert.StartsWith(storePath + ".commit-pending.tmp.", pendingTempPath, StringComparison.Ordinal);
            Assert.NotEqual(storePath + ".tmp", candidateTempPath);
            Assert.True(tempMatchedCandidate);
            Assert.True(pendingGuardExisted);
            Assert.True(primaryMatchedCandidate);
            Assert.False(File.Exists(candidateTempPath));
            Assert.False(File.Exists(pendingTempPath));
            Assert.False(File.Exists(storePath + ".commit-pending"));
            Assert.Empty(Directory.EnumerateFiles(root, "jobs.json.commit-pending.tmp.*"));
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void PreReplaceFailureReportsNotCommittedAndPreservesPreviousPrimary()
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        var previous = EmptyStoreJson(version: 1);
        var candidate = EmptyStoreJson(version: 2);
        Directory.CreateDirectory(root);
        File.WriteAllText(storePath, previous);

        try
        {
            var store = new JsonFileDesktopNodeJobStore(
                storePath,
                (stage, _) =>
                {
                    if (stage == JsonFileDesktopNodeJobStoreWriteStage.PendingCommitFlushed)
                    {
                        throw new IOException("Injected failure before primary replacement.");
                    }
                });

            var result = store.WriteSnapshot(candidate);

            Assert.Equal(DesktopNodeJobStoreCommitOutcome.NotCommitted, result.Outcome);
            Assert.IsType<IOException>(result.Failure);
            Assert.Equal(previous, File.ReadAllText(storePath));
            Assert.False(File.Exists(storePath + ".commit-pending"));
            Assert.Empty(Directory.EnumerateFiles(root, "jobs.json.tmp.*"));
            Assert.Empty(Directory.EnumerateFiles(root, "jobs.json.commit-pending.tmp.*"));
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void PendingCommitPublicationFailureIsNotCommittedAndLeavesNoAuthoritativeGuard()
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        var previous = EmptyStoreJson(version: 1);
        var candidate = EmptyStoreJson(version: 2);
        Directory.CreateDirectory(root);
        File.WriteAllText(storePath, previous);

        try
        {
            var store = new JsonFileDesktopNodeJobStore(
                storePath,
                (stage, _) =>
                {
                    if (stage == JsonFileDesktopNodeJobStoreWriteStage.PendingCommitTempFlushed)
                    {
                        throw new IOException("Injected failure before pending-commit publication.");
                    }
                });

            var result = store.WriteSnapshot(candidate);

            Assert.Equal(DesktopNodeJobStoreCommitOutcome.NotCommitted, result.Outcome);
            Assert.IsType<IOException>(result.Failure);
            Assert.Equal(previous, File.ReadAllText(storePath));
            Assert.False(File.Exists(storePath + ".commit-pending"));
            Assert.Empty(Directory.EnumerateFiles(root, "jobs.json.tmp.*"));
            Assert.Empty(Directory.EnumerateFiles(root, "jobs.json.commit-pending.tmp.*"));
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void PostReplaceFailureWithMatchingPrimaryIsReconciledAsCommitted()
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        var candidate = EmptyStoreJson(version: 2);
        Directory.CreateDirectory(root);

        try
        {
            var store = new JsonFileDesktopNodeJobStore(
                storePath,
                (stage, _) =>
                {
                    if (stage == JsonFileDesktopNodeJobStoreWriteStage.PrimaryReplaced)
                    {
                        throw new IOException("Injected failure after primary replacement.");
                    }
                });

            var result = store.WriteSnapshot(candidate);

            Assert.Equal(DesktopNodeJobStoreCommitOutcome.Committed, result.Outcome);
            Assert.Null(result.Failure);
            Assert.Equal(candidate, File.ReadAllText(storePath));
            Assert.False(File.Exists(storePath + ".commit-pending"));
            Assert.Empty(Directory.EnumerateFiles(root, "jobs.json.tmp.*"));
            Assert.Empty(Directory.EnumerateFiles(root, "jobs.json.commit-pending.tmp.*"));
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void IndeterminatePostReplaceFailureBlocksDispatchUntilRestartReconcilesPrimary()
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        FileStream? primaryLock = null;
        Directory.CreateDirectory(root);

        try
        {
            var store = new JsonFileDesktopNodeJobStore(
                storePath,
                (stage, _) =>
                {
                    if (stage != JsonFileDesktopNodeJobStoreWriteStage.PrimaryReplaced)
                    {
                        return;
                    }

                    primaryLock = new FileStream(
                        storePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.None);
                    throw new IOException("Injected unreadable-primary failure after replacement.");
                });
            var runtime = new DesktopNodeJobRuntime(store);

            var exception = Assert.Throws<DesktopNodeJobStoreWriteException>(() =>
                CreateJob(runtime, "job-indeterminate"));

            Assert.Equal(DesktopNodeJobStoreCommitOutcome.Indeterminate, exception.CommitOutcome);
            Assert.Equal("PCV_JOB_STORE_SAVE_FAILED", exception.Error.Code);
            Assert.NotNull(runtime.LoadBlock);
            Assert.Empty(runtime.Snapshot().Jobs);
            Assert.Empty(runtime.Snapshot().Queue);
            Assert.Null(runtime.TryStartNext(() => { }));
            Assert.True(File.Exists(storePath + ".commit-pending"));

            var blockedRestart = DesktopNodeJobRuntime.CreateDefault(storePath);
            Assert.NotNull(blockedRestart.LoadBlock);
            Assert.Empty(blockedRestart.Snapshot().Jobs);
            Assert.Null(blockedRestart.TryStartNext(() => { }));

            primaryLock!.Dispose();
            primaryLock = null;

            var reconciledRestart = DesktopNodeJobRuntime.CreateDefault(storePath);
            Assert.Null(reconciledRestart.LoadBlock);
            Assert.Equal("job-indeterminate", Assert.Single(reconciledRestart.Snapshot().Jobs).JobId);
            Assert.Equal(["job-indeterminate"], reconciledRestart.Snapshot().Queue);
            Assert.False(File.Exists(storePath + ".commit-pending"));
        }
        finally
        {
            primaryLock?.Dispose();
            DeleteRoot(root);
        }
    }

    [Fact]
    public void PendingPreReplaceCommitBlocksUntilRestartConfirmsPreviousPrimary()
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        var pendingPath = storePath + ".commit-pending";
        FileStream? pendingLock = null;
        Directory.CreateDirectory(root);
        File.WriteAllText(storePath, StoreJsonWithQueuedJob("job-existing", version: 1));

        try
        {
            var store = new JsonFileDesktopNodeJobStore(
                storePath,
                (stage, _) =>
                {
                    if (stage != JsonFileDesktopNodeJobStoreWriteStage.PendingCommitFlushed)
                    {
                        return;
                    }

                    pendingLock = new FileStream(
                        pendingPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.None);
                    throw new IOException("Injected failure while the pending guard is locked.");
                });

            var result = store.WriteSnapshot(StoreJsonWithQueuedJob("job-candidate", version: 1));

            Assert.Equal(DesktopNodeJobStoreCommitOutcome.Indeterminate, result.Outcome);
            Assert.True(File.Exists(pendingPath));
            Assert.Contains("job-existing", File.ReadAllText(storePath), StringComparison.Ordinal);
            Assert.DoesNotContain("job-candidate", File.ReadAllText(storePath), StringComparison.Ordinal);

            var blockedRestart = DesktopNodeJobRuntime.CreateDefault(storePath);
            Assert.NotNull(blockedRestart.LoadBlock);
            Assert.Empty(blockedRestart.Snapshot().Jobs);
            Assert.Null(blockedRestart.TryStartNext(() => { }));

            pendingLock!.Dispose();
            pendingLock = null;

            var reconciledRestart = DesktopNodeJobRuntime.CreateDefault(storePath);
            Assert.Null(reconciledRestart.LoadBlock);
            Assert.Equal("job-existing", Assert.Single(reconciledRestart.Snapshot().Jobs).JobId);
            Assert.Equal(["job-existing"], reconciledRestart.Snapshot().Queue);
            Assert.False(File.Exists(pendingPath));
        }
        finally
        {
            pendingLock?.Dispose();
            DeleteRoot(root);
        }
    }

    [Fact]
    public void PendingFirstWriteReconcilesConfirmedAbsentPreviousSnapshot()
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        var pendingPath = storePath + ".commit-pending";
        FileStream? pendingLock = null;
        Directory.CreateDirectory(root);

        try
        {
            var store = new JsonFileDesktopNodeJobStore(
                storePath,
                (stage, _) =>
                {
                    if (stage != JsonFileDesktopNodeJobStoreWriteStage.PendingCommitFlushed)
                    {
                        return;
                    }

                    pendingLock = new FileStream(
                        pendingPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.None);
                    throw new IOException("Injected first-write failure while the pending guard is locked.");
                });

            var result = store.WriteSnapshot(StoreJsonWithQueuedJob("job-candidate", version: 1));

            Assert.Equal(DesktopNodeJobStoreCommitOutcome.Indeterminate, result.Outcome);
            Assert.False(File.Exists(storePath));
            Assert.True(File.Exists(pendingPath));

            var blockedRestart = DesktopNodeJobRuntime.CreateDefault(storePath);
            Assert.NotNull(blockedRestart.LoadBlock);
            Assert.Empty(blockedRestart.Snapshot().Jobs);
            Assert.Null(blockedRestart.TryStartNext(() => { }));

            pendingLock!.Dispose();
            pendingLock = null;

            var reconciledRestart = DesktopNodeJobRuntime.CreateDefault(storePath);
            Assert.Null(reconciledRestart.LoadBlock);
            Assert.Empty(reconciledRestart.Snapshot().Jobs);
            Assert.Empty(reconciledRestart.Snapshot().Queue);
            Assert.False(File.Exists(storePath));
            Assert.False(File.Exists(pendingPath));
        }
        finally
        {
            pendingLock?.Dispose();
            DeleteRoot(root);
        }
    }

    [Fact]
    public void InvalidPendingCommitBlocksWithoutLoadingOrMutatingPrimary()
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        var pendingPath = storePath + ".commit-pending";
        var primary = StoreJsonWithQueuedJob("job-existing", version: 1);
        Directory.CreateDirectory(root);
        File.WriteAllText(storePath, primary);
        File.WriteAllText(pendingPath, "{\"version\":1,\"candidate_sha256\":\"invalid\"}");

        try
        {
            var blocked = DesktopNodeJobRuntime.CreateDefault(storePath);

            Assert.NotNull(blocked.LoadBlock);
            Assert.Empty(blocked.Snapshot().Jobs);
            Assert.Empty(blocked.Snapshot().Queue);
            Assert.Null(blocked.TryStartNext(() => { }));
            Assert.Equal(primary, File.ReadAllText(storePath));
            Assert.True(File.Exists(pendingPath));
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void IdentityMatchedCorruptPrimaryPreservesPendingGuardAndBlocksStartup()
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        var pendingPath = storePath + ".commit-pending";
        FileStream? pendingLock = null;
        Directory.CreateDirectory(root);

        try
        {
            var store = new JsonFileDesktopNodeJobStore(
                storePath,
                (stage, _) =>
                {
                    if (stage != JsonFileDesktopNodeJobStoreWriteStage.PrimaryReplaced)
                    {
                        return;
                    }

                    pendingLock = new FileStream(
                        pendingPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.None);
                    throw new IOException("Injected marker retention after corrupt primary replacement.");
                });

            var result = store.WriteSnapshot("{not-json");
            Assert.Equal(DesktopNodeJobStoreCommitOutcome.Indeterminate, result.Outcome);
            Assert.True(File.Exists(pendingPath));
            Assert.Equal("{not-json", File.ReadAllText(storePath));

            pendingLock!.Dispose();
            pendingLock = null;

            var blocked = DesktopNodeJobRuntime.CreateDefault(storePath);
            Assert.Equal("PCV_JOB_STORE_LOAD_FAILED", blocked.LoadBlock!.Code);
            Assert.Empty(blocked.Snapshot().Jobs);
            Assert.Empty(blocked.Snapshot().Queue);
            Assert.True(File.Exists(pendingPath));
            Assert.Equal("{not-json", File.ReadAllText(storePath));
        }
        finally
        {
            pendingLock?.Dispose();
            DeleteRoot(root);
        }
    }

    [Fact]
    public void InvalidUtf8PrimaryStartsInCorruptBlockedStateWithoutRewrite()
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        Directory.CreateDirectory(root);
        var bytes = Encoding.UTF8.GetBytes(StoreJsonWithQueuedJob("job-invalid-utf8", version: 1));
        var jobIdOffset = bytes.AsSpan().IndexOf(Encoding.UTF8.GetBytes("job-invalid-utf8"));
        Assert.True(jobIdOffset >= 0);
        bytes[jobIdOffset + 4] = 0xff;
        File.WriteAllBytes(storePath, bytes);

        try
        {
            var beforeHash = SHA256.HashData(File.ReadAllBytes(storePath));

            var blocked = DesktopNodeJobRuntime.CreateDefault(storePath);

            Assert.Equal("PCV_JOB_STORE_CORRUPT", blocked.LoadBlock!.Code);
            Assert.Empty(blocked.Snapshot().Jobs);
            Assert.Empty(blocked.Snapshot().Queue);
            Assert.Null(blocked.TryStartNext(() => { }));
            Assert.Equal(beforeHash, SHA256.HashData(File.ReadAllBytes(storePath)));
            Assert.False(File.Exists(storePath + ".commit-pending"));
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void IdentityMatchedInvalidUtf8PrimaryPreservesPendingGuard()
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        var pendingPath = storePath + ".commit-pending";
        Directory.CreateDirectory(root);
        var bytes = Encoding.UTF8.GetBytes(StoreJsonWithQueuedJob("job-invalid-utf8-pending", version: 1));
        var jobIdOffset = bytes.AsSpan().IndexOf(Encoding.UTF8.GetBytes("job-invalid-utf8-pending"));
        Assert.True(jobIdOffset >= 0);
        bytes[jobIdOffset + 4] = 0xff;
        File.WriteAllBytes(storePath, bytes);
        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        File.WriteAllText(
            pendingPath,
            JsonSerializer.Serialize(new SortedDictionary<string, object?>
            {
                ["candidate_length"] = bytes.LongLength,
                ["candidate_sha256"] = hash,
                ["previous_exists"] = false,
                ["previous_length"] = null,
                ["previous_sha256"] = null,
                ["version"] = 1
            }));

        try
        {
            var blocked = DesktopNodeJobRuntime.CreateDefault(storePath);

            Assert.Equal("PCV_JOB_STORE_LOAD_FAILED", blocked.LoadBlock!.Code);
            Assert.Empty(blocked.Snapshot().Jobs);
            Assert.Empty(blocked.Snapshot().Queue);
            Assert.True(File.Exists(pendingPath));
            Assert.Equal(bytes, File.ReadAllBytes(storePath));
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void InaccessiblePrimaryStartsStructuredBlockedWithoutTreatingItAsMissing()
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        var primary = StoreJsonWithQueuedJob("job-existing", version: 1);
        Directory.CreateDirectory(root);
        File.WriteAllText(storePath, primary);

        try
        {
            using (var primaryLock = new FileStream(
                storePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None))
            {
                var blocked = DesktopNodeJobRuntime.CreateDefault(storePath);

                Assert.NotNull(blocked.LoadBlock);
                Assert.Equal("PCV_JOB_STORE_LOAD_FAILED", blocked.LoadBlock.Code);
                Assert.DoesNotContain(
                    "candidate snapshot was not published",
                    blocked.LoadBlock.Detail,
                    StringComparison.Ordinal);
                Assert.Empty(blocked.Snapshot().Jobs);
                Assert.Empty(blocked.Snapshot().Queue);
                Assert.Null(blocked.TryStartNext(() => { }));
            }

            Assert.Equal(primary, File.ReadAllText(storePath));
            var restarted = DesktopNodeJobRuntime.CreateDefault(storePath);
            Assert.Null(restarted.LoadBlock);
            Assert.Equal("job-existing", Assert.Single(restarted.Snapshot().Jobs).JobId);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StartupNeverPromotesOrphanUniqueTemp(bool primaryExists)
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        var orphanPath = storePath + ".tmp." + Guid.NewGuid().ToString("N");
        var orphanPendingTempPath = storePath + ".commit-pending.tmp." + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(root);
        File.WriteAllText(orphanPath, StoreJsonWithQueuedJob("job-orphan", version: 1));
        File.WriteAllText(orphanPendingTempPath, "partial-non-authoritative-marker");
        if (primaryExists)
        {
            File.WriteAllText(storePath, StoreJsonWithQueuedJob("job-primary", version: 1));
        }

        try
        {
            var runtime = DesktopNodeJobRuntime.CreateDefault(storePath);
            var snapshot = runtime.Snapshot();

            if (primaryExists)
            {
                Assert.Equal("job-primary", Assert.Single(snapshot.Jobs).JobId);
                Assert.Equal(["job-primary"], snapshot.Queue);
            }
            else
            {
                Assert.Empty(snapshot.Jobs);
                Assert.Empty(snapshot.Queue);
            }

            Assert.DoesNotContain(snapshot.Jobs, job => job.JobId == "job-orphan");
            Assert.True(File.Exists(orphanPath));
            Assert.True(File.Exists(orphanPendingTempPath));
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void DurableWriterPreserves04265CompatibleV1AndV2Shape(int version)
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        Directory.CreateDirectory(root);
        File.WriteAllText(storePath, EmptyStoreJson(version));

        try
        {
            var runtime = DesktopNodeJobRuntime.CreateDefault(storePath);
            CreateJob(runtime, $"job-v{version}");
            var bytes = File.ReadAllBytes(storePath);

            Assert.False(bytes.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf }));
            var compatibleShape = ReadWith04265ShapeContract(bytes);
            Assert.Equal(version, compatibleShape.Version);
            Assert.Equal($"job-v{version}", compatibleShape.JobId);
            Assert.Equal([$"job-v{version}"], compatibleShape.Queue);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void StaleLoadedBaseIsRejectedWithoutOverwritingNewerPrimary()
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        var firstCandidate = StoreJsonWithQueuedJob("job-first-writer", version: 1);
        var staleCandidate = StoreJsonWithQueuedJob("job-stale-writer", version: 1);
        Directory.CreateDirectory(root);

        try
        {
            var first = new JsonFileDesktopNodeJobStore(storePath);
            var stale = new JsonFileDesktopNodeJobStore(storePath);
            Assert.False(first.Exists());
            Assert.False(stale.Exists());

            var firstResult = first.WriteSnapshot(firstCandidate);
            var staleResult = stale.WriteSnapshot(staleCandidate);

            Assert.Equal(DesktopNodeJobStoreCommitOutcome.Committed, firstResult.Outcome);
            Assert.Equal(DesktopNodeJobStoreCommitOutcome.NotCommitted, staleResult.Outcome);
            Assert.IsType<DesktopNodeJobStoreConcurrencyException>(staleResult.Failure);
            Assert.Equal(firstCandidate, File.ReadAllText(storePath));
            Assert.False(File.Exists(storePath + ".commit-pending"));
            Assert.Empty(Directory.EnumerateFiles(root, "jobs.json.tmp.*"));
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void StaleQuarantineIsRejectedWithoutMovingNewerPrimary()
    {
        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        var original = StoreJsonWithQueuedJob("job-original", version: 1);
        var newer = StoreJsonWithQueuedJob("job-newer", version: 1);
        Directory.CreateDirectory(root);
        File.WriteAllText(storePath, original);

        try
        {
            var writer = new JsonFileDesktopNodeJobStore(storePath);
            var stale = new JsonFileDesktopNodeJobStore(storePath);
            Assert.Equal(original, writer.ReadSnapshot());
            Assert.Equal(original, stale.ReadSnapshot());
            Assert.Equal(
                DesktopNodeJobStoreCommitOutcome.Committed,
                writer.WriteSnapshot(newer).Outcome);

            Assert.Throws<DesktopNodeJobStoreConcurrencyException>(() =>
                stale.Quarantine(".corrupt"));

            Assert.Equal(newer, File.ReadAllText(storePath));
            Assert.False(File.Exists(storePath + ".corrupt"));
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void CanonicalPathAliasesShareOneCrossProcessWriteLeaseName()
    {
        var root = NewRoot();
        var directPath = Path.Combine(root, "jobs.json");
        var aliasPath = Path.Combine(root, "nested", "..", "jobs.json");

        Assert.Equal(
            JsonFileDesktopNodeJobStore.CreateWriteLeaseName(directPath),
            JsonFileDesktopNodeJobStore.CreateWriteLeaseName(aliasPath));
    }

    [Fact]
    public void WindowsStoreRejectsUncLocation()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var exception = Assert.Throws<ArgumentException>(() =>
            new JsonFileDesktopNodeJobStore(@"\\server\share\jobs.json"));

        Assert.Contains("UNC", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(DriveType.Network)]
    [InlineData(DriveType.Removable)]
    [InlineData(DriveType.Ram)]
    [InlineData(DriveType.CDRom)]
    [InlineData(DriveType.Unknown)]
    public void WindowsStoreRejectsNonFixedDriveTypes(DriveType driveType)
    {
        Assert.False(JsonFileDesktopNodeJobStore.IsSupportedWindowsDriveType(driveType));
        Assert.True(JsonFileDesktopNodeJobStore.IsSupportedWindowsDriveType(DriveType.Fixed));
    }

    [Fact]
    public void WindowsStoreRejectsObviousDosShortNameAlias()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var path = Path.Combine(NewRoot(), "PROGRA~1", "jobs.json");

        var exception = Assert.Throws<ArgumentException>(() =>
            new JsonFileDesktopNodeJobStore(path));

        Assert.Contains("8.3", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WindowsStoreRejectsAlternateDataStreamLocation()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var path = Path.Combine(NewRoot(), "jobs.json") + "::$DATA";

        var exception = Assert.Throws<ArgumentException>(() =>
            new JsonFileDesktopNodeJobStore(path));

        Assert.Contains("alternate data stream", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void VolumeRelativeLeaseIdentityConvergesAcrossDriveLetters()
    {
        const string volume = @"\\?\Volume{11111111-2222-3333-4444-555555555555}\";

        var first = JsonFileDesktopNodeJobStore.CreateVolumeRelativeLeaseIdentity(
            volume,
            @"ProgramData\PureCVisor\desktop-node\jobs.json");
        var second = JsonFileDesktopNodeJobStore.CreateVolumeRelativeLeaseIdentity(
            volume.ToLowerInvariant(),
            @"programdata/purecvisor/desktop-node/jobs.json");

        Assert.Equal(first, second);
        Assert.DoesNotContain("C:", first, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("X:", first, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(@"\??\C:\data", true)]
    [InlineData(@"\??\Volume{11111111-2222-3333-4444-555555555555}\", true)]
    [InlineData(@"\Device\HarddiskVolume4", false)]
    public void SubstDeviceTargetsAreRejectedByMappingPolicy(string target, bool expected)
    {
        Assert.Equal(expected, JsonFileDesktopNodeJobStore.IsSubstDeviceTarget(target));
    }

    [Fact]
    public void HardLinkedPrimaryIsRejectedBeforeWriterCanSplitAuthoritativeState()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var root = NewRoot();
        var storePath = Path.Combine(root, "jobs.json");
        var aliasPath = Path.Combine(root, "jobs-alias.json");
        var original = StoreJsonWithQueuedJob("job-original", version: 1);
        Directory.CreateDirectory(root);
        File.WriteAllText(storePath, original);
        try
        {
            if (!CreateHardLink(aliasPath, storePath, IntPtr.Zero))
            {
                return;
            }

            var store = new JsonFileDesktopNodeJobStore(storePath);
            Assert.Equal(original, store.ReadSnapshot());

            var result = store.WriteSnapshot(
                StoreJsonWithQueuedJob("job-replacement", version: 1));

            Assert.Equal(DesktopNodeJobStoreCommitOutcome.NotCommitted, result.Outcome);
            Assert.IsType<DesktopNodeJobStoreConcurrencyException>(result.Failure);
            Assert.Equal(original, File.ReadAllText(storePath));
            Assert.Equal(original, File.ReadAllText(aliasPath));
            Assert.False(File.Exists(storePath + ".commit-pending"));
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void WindowsStoreRejectsExistingDirectoryReparsePointAncestor()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var root = NewRoot();
        var target = Path.Combine(root, "target");
        var alias = Path.Combine(root, "alias");
        Directory.CreateDirectory(target);

        try
        {
            if (!TryCreateDirectorySymbolicLink(alias, target))
            {
                return;
            }

            var exception = Assert.Throws<ArgumentException>(() =>
                new JsonFileDesktopNodeJobStore(Path.Combine(alias, "jobs.json")));

            Assert.Contains("reparse point", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void WindowsStoreRejectsExistingFileReparsePoint()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var root = NewRoot();
        var target = Path.Combine(root, "target.json");
        var alias = Path.Combine(root, "jobs.json");
        Directory.CreateDirectory(root);
        File.WriteAllText(target, EmptyStoreJson(version: 1));

        try
        {
            if (!TryCreateFileSymbolicLink(alias, target))
            {
                return;
            }

            var exception = Assert.Throws<ArgumentException>(() =>
                new JsonFileDesktopNodeJobStore(alias));

            Assert.Contains("reparse point", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [Fact]
    public void DefaultStoreDoesNotUseOccupiedFixedTempPathForNewWrites()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pcv-runtime-job-store-" + Guid.NewGuid().ToString("N"));
        var storePath = Path.Combine(root, "jobs.json");
        var legacyFixedTempPath = storePath + ".tmp";
        Directory.CreateDirectory(legacyFixedTempPath);

        try
        {
            var runtime = DesktopNodeJobRuntime.CreateDefault(storePath);

            var created = runtime.Create(
                new DesktopNodeJobCreateCommand(
                    "vm.start",
                    JsonSerializer.SerializeToElement(new SortedDictionary<string, object?>
                    {
                        ["name"] = "alpha"
                    }),
                    JobId: "job-unique-temp"),
                new DesktopNodeJobRequestContext("req-unique-temp"));

            Assert.Equal("job-unique-temp", created.JobId);
            Assert.True(File.Exists(storePath));
            Assert.True(Directory.Exists(legacyFixedTempPath));
            Assert.Empty(Directory.EnumerateFiles(root, "jobs.json.tmp.*"));
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
    public void DefaultStoreReplacesStaleTempAndLoadsWrittenQueuedJob()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "pcv-runtime-job-store-" + Guid.NewGuid().ToString("N"));
        var storePath = Path.Combine(root, "jobs.json");
        var tempPath = storePath + ".tmp";
        Directory.CreateDirectory(root);
        File.WriteAllText(tempPath, "stale-temp");

        try
        {
            var runtime = DesktopNodeJobRuntime.CreateDefault(storePath);
            var created = runtime.Create(
                new DesktopNodeJobCreateCommand(
                    "vm.start",
                    JsonSerializer.SerializeToElement(new SortedDictionary<string, object?>
                    {
                        ["name"] = "alpha"
                    }),
                    JobId: "job-physical-store"),
                new DesktopNodeJobRequestContext("req-physical-store"));

            Assert.Equal("job-physical-store", created.JobId);
            Assert.True(File.Exists(storePath));
            Assert.False(File.Exists(tempPath));
            using (var document = JsonDocument.Parse(File.ReadAllText(storePath)))
            {
                Assert.Equal(1, document.RootElement.GetProperty("version").GetInt32());
                Assert.Equal("job-physical-store", document.RootElement.GetProperty("queue")[0].GetString());
                Assert.Equal("queued", document.RootElement.GetProperty("jobs")[0].GetProperty("status").GetString());
            }

            var restarted = DesktopNodeJobRuntime.CreateDefault(storePath);
            var state = restarted.Snapshot();
            Assert.Equal("job-physical-store", Assert.Single(state.Jobs).JobId);
            Assert.Equal(["job-physical-store"], state.Queue);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static DesktopNodeJobSnapshot CreateJob(
        DesktopNodeJobRuntime runtime,
        string jobId)
    {
        return runtime.Create(
            new DesktopNodeJobCreateCommand(
                "vm.start",
                JsonSerializer.SerializeToElement(new SortedDictionary<string, object?>
                {
                    ["name"] = "alpha"
                }),
                JobId: jobId),
            new DesktopNodeJobRequestContext("req-" + jobId));
    }

    private static string EmptyStoreJson(int version)
    {
        return JsonSerializer.Serialize(new SortedDictionary<string, object?>
        {
            ["jobs"] = Array.Empty<object>(),
            ["queue"] = Array.Empty<string>(),
            ["saved_at"] = "2026-08-02T00:00:00.0000000+00:00",
            ["version"] = version
        });
    }

    private static string StoreJsonWithQueuedJob(string jobId, int version)
    {
        var timestamp = "2026-08-02T00:00:00.0000000+00:00";
        return JsonSerializer.Serialize(new SortedDictionary<string, object?>
        {
            ["jobs"] = new[]
            {
                new SortedDictionary<string, object?>
                {
                    ["attempt"] = 1,
                    ["canceled_at"] = null,
                    ["correlation_id"] = jobId,
                    ["created_at"] = timestamp,
                    ["error"] = null,
                    ["job_id"] = jobId,
                    ["operation"] = "vm.start",
                    ["params"] = new SortedDictionary<string, object?> { ["name"] = "alpha" },
                    ["request_id"] = "req-" + jobId,
                    ["result"] = null,
                    ["retry_of"] = null,
                    ["status"] = "queued",
                    ["updated_at"] = timestamp
                }
            },
            ["queue"] = new[] { jobId },
            ["saved_at"] = timestamp,
            ["version"] = version
        });
    }

    private static Compatible04265ShapeProjection ReadWith04265ShapeContract(byte[] bytes)
    {
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.Equal(
            ["jobs", "queue", "saved_at", "version"],
            root.EnumerateObject().Select(property => property.Name).Order(StringComparer.Ordinal));
        var job = Assert.Single(root.GetProperty("jobs").EnumerateArray());
        Assert.Equal(
            [
                "attempt", "canceled_at", "correlation_id", "created_at", "error", "job_id",
                "operation", "params", "request_id", "result", "retry_of", "status", "updated_at"
            ],
            job.EnumerateObject().Select(property => property.Name).Order(StringComparer.Ordinal));
        return new Compatible04265ShapeProjection(
            root.GetProperty("version").GetInt32(),
            job.GetProperty("job_id").GetString()!,
            root.GetProperty("queue").EnumerateArray().Select(item => item.GetString()!).ToArray());
    }

    private static string NewRoot()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "pcv-runtime-job-store-" + Guid.NewGuid().ToString("N"));
    }

    private static void DeleteRoot(string root)
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static bool TryCreateDirectorySymbolicLink(string path, string target)
    {
        try
        {
            Directory.CreateSymbolicLink(path, target);
            return true;
        }
        catch (Exception exception) when (
            exception is UnauthorizedAccessException or
                IOException or
                PlatformNotSupportedException)
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

    private static bool TryCreateFileSymbolicLink(string path, string target)
    {
        try
        {
            File.CreateSymbolicLink(path, target);
            return true;
        }
        catch (Exception exception) when (
            exception is UnauthorizedAccessException or
                IOException or
                PlatformNotSupportedException)
        {
            return false;
        }
    }

    private sealed record Compatible04265ShapeProjection(
        int Version,
        string JobId,
        string[] Queue);
}
