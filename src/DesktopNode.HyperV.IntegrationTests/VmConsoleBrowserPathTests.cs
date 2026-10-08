using System.Security.Cryptography;
using System.Text.Json;

namespace DesktopNode.HyperV.IntegrationTests;

// S1 browser console (design pcv-s1-browser-console-v1 §7): the product adapter reads a pcv-it- VM's firmware screen and
// sends Msvm_Keyboard input to it. No guest OS is installed or signed in to; the VM is powered off and deleted at the end.
public sealed class VmConsoleBrowserPathTests(HyperVIntegrationFixture fixture) : IClassFixture<HyperVIntegrationFixture>
{
    [Fact]
    public void FirmwareScreenIsReadableAndKeyboardInputChangesIt()
    {
        var name = fixture.VmName("console");
        var record = new SortedDictionary<string, object?> { ["vm"] = name };
        try
        {
            var created = fixture.Invoke("vm.create", new
            {
                name,
                iso_path = fixture.IsoPath,
                cpu = 1,
                memory_mb = 1024,
                disk_gb = 8,
                vm_root = fixture.VmRoot,
                generation = 2
            });
            Assert.True(created.Ok, $"{created.Error?.Code}: {created.Error?.Message}");
            var started = fixture.Invoke("vm.start", new { name });
            Assert.True(started.Ok, $"{started.Error?.Code}: {started.Error?.Message}");

            var stable = WaitForStableFrame(name, TimeSpan.FromSeconds(90), record);
            Assert.True(NonBlackPixels(stable) > 0, "the firmware screen should not be blank");

            foreach (var (label, input) in new (string, object)[]
            {
                ("esc", new { name, kind = "key", action = "type", key_code = 27 }),
                ("shift-press", new { name, kind = "key", action = "press", key_code = 16 }),
                ("shift-release", new { name, kind = "key", action = "release", key_code = 16 }),
                ("text", new { name, kind = "text", text = "a" })
            })
            {
                var sent = fixture.Invoke("vm.console.input", input);
                record[$"input_{label}"] = sent.Ok ? "ok" : $"{sent.Error?.Code}: {sent.Error?.Detail}";
                Assert.True(sent.Ok, $"{label}: {sent.Error?.Code} {sent.Error?.Detail}");
            }

            var before = ReadFrame(name);
            var reset = fixture.Invoke("vm.console.input", new { name, kind = "ctrl-alt-del" });
            record["input_ctrl_alt_del"] = reset.Ok ? "ok" : $"{reset.Error?.Code}: {reset.Error?.Detail}";
            Assert.True(reset.Ok, $"ctrl-alt-del: {reset.Error?.Code} {reset.Error?.Detail}");

            var changedAfter = WaitForChange(name, before, TimeSpan.FromSeconds(20));
            record["frame_changed_after_ctrl_alt_del_ms"] = changedAfter?.TotalMilliseconds;
            Assert.True(changedAfter is not null, "the screen should change after Ctrl+Alt+Del resets the firmware");
        }
        finally
        {
            if (fixture.FindVm(name) is not null)
            {
                fixture.Invoke("vm.poweroff", new { name });
            }

            fixture.DeleteVm(name);
            record["deleted"] = fixture.FindVm(name) is null;
            var recordRoot = Path.Combine(Path.GetDirectoryName(fixture.VmRoot)!, "console-browser-path");
            Directory.CreateDirectory(recordRoot);
            File.WriteAllText(
                Path.Combine(recordRoot, fixture.RunId + ".json"),
                JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
    }

    private byte[] ReadFrame(string name)
    {
        var frame = fixture.Invoke("vm.console.frame", new { name, width = 640, height = 480 });
        Assert.True(frame.Ok, $"{frame.Error?.Code}: {frame.Error?.Detail}");
        return Convert.FromBase64String(frame.Data!.Value.GetProperty("frame_base64").GetString()!);
    }

    private byte[] WaitForStableFrame(string name, TimeSpan timeout, SortedDictionary<string, object?> record)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var previous = string.Empty;
        var repeats = 0;
        var timings = new List<long>();
        while (true)
        {
            var read = System.Diagnostics.Stopwatch.StartNew();
            var frame = ReadFrame(name);
            timings.Add(read.ElapsedMilliseconds);
            var hash = Convert.ToHexString(SHA256.HashData(frame));
            repeats = hash == previous && NonBlackPixels(frame) > 0 ? repeats + 1 : 0;
            previous = hash;
            if (repeats >= 2 || watch.Elapsed > timeout)
            {
                record["stable_after_ms"] = watch.ElapsedMilliseconds;
                record["frame_read_ms_max"] = timings.Max();
                record["frame_reads"] = timings.Count;
                record["nonblack_pixels"] = NonBlackPixels(frame);
                return frame;
            }

            Thread.Sleep(1000);
        }
    }

    private TimeSpan? WaitForChange(string name, byte[] before, TimeSpan timeout)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var baseline = Convert.ToHexString(SHA256.HashData(before));
        while (watch.Elapsed < timeout)
        {
            Thread.Sleep(500);
            if (Convert.ToHexString(SHA256.HashData(ReadFrame(name))) != baseline)
            {
                return watch.Elapsed;
            }
        }

        return null;
    }

    private static int NonBlackPixels(byte[] frame)
    {
        var count = 0;
        for (var index = 0; index + 1 < frame.Length; index += 2)
        {
            if (frame[index] != 0 || frame[index + 1] != 0)
            {
                count++;
            }
        }

        return count;
    }
}
