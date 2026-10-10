using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Host;

namespace DesktopNode.Host.Tests;

public sealed class DesktopNodeHostLoopbackBootstrapBrowserTests
{
    private const string ServiceTokenValue = "loopback-browser-gate-service-secret";

    [Fact]
    public async Task ChromiumOpensLoopbackConsoleWithoutServiceTokenPaste()
    {
        var browser = FindBrowser();
        var webRoot = FindWebRoot();
        var tokenFile = WriteServiceTokenFile();
        using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.Listen,
            Prefix = "http://127.0.0.1:0/",
            WebPrefix = "http://127.0.0.1:0/",
            WebRootPath = webRoot,
            ApiTokenFile = tokenFile,
            AccountAuthOptions = new DesktopNodeAccountAuthOptions(
                Enabled: true,
                Issuer: "pcv-test",
                Audience: "pcv-local-api",
                SigningKey: SyntheticAuthMaterial.Value,
                Accounts: [])
        });

        var debugPort = GetFreeLoopbackPort();
        var userData = Directory.CreateTempSubdirectory("pcv-loopback-browser-").FullName;
        Process? process = null;
        try
        {
            process = StartBrowser(browser, debugPort, userData);
            await WaitForDevToolsAsync(debugPort);
            var snapshot = await OpenAndReadAsync(host.WebBaseUri, debugPort);

            Assert.False(string.IsNullOrWhiteSpace(snapshot.Session));
            Assert.Contains("access_token", snapshot.Session, StringComparison.Ordinal);
            Assert.DoesNotContain("Auth required", snapshot.Connection, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("VM: 3/3", snapshot.StatusVm, StringComparison.Ordinal);
            Assert.DoesNotContain("pcv-node-a", snapshot.BodyText, StringComparison.Ordinal);
            Assert.False(snapshot.AuthGate);
            Assert.Equal("vms", snapshot.ShownView);
            Assert.DoesNotContain(ServiceTokenValue, snapshot.BodyText, StringComparison.Ordinal);
        }
        finally
        {
            // msedge.exe can hand the browser to a new process and exit, so the started
            // process is not a reliable handle. Close through DevTools and the user-data-dir.
            await CloseBrowserAsync(debugPort, userData);
            if (process is not null && !process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException)
                {
                }
            }

            process?.Dispose();
            DeleteDirectoryWithRetry(userData);
            try
            {
                File.Delete(tokenFile);
            }
            catch (IOException)
            {
            }
        }

        Assert.Empty(FindBrowserProcessIds(userData));
    }

    private static async Task CloseBrowserAsync(int debugPort, string userData)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            using var version = JsonDocument.Parse(await http.GetStringAsync($"http://127.0.0.1:{debugPort}/json/version"));
            var browserUrl = version.RootElement.GetProperty("webSocketDebuggerUrl").GetString();
            if (!string.IsNullOrWhiteSpace(browserUrl))
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                using var webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri(browserUrl), timeout.Token);
                await webSocket.SendAsync(
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { id = 1, method = "Browser.close" })),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    timeout.Token);
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException or
            WebSocketException or JsonException or KeyNotFoundException or InvalidOperationException or UriFormatException)
        {
        }

        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline && FindBrowserProcessIds(userData).Count > 0)
        {
            await Task.Delay(200);
        }

        foreach (var processId in FindBrowserProcessIds(userData))
        {
            try
            {
                using var leftover = Process.GetProcessById(processId);
                leftover.Kill(entireProcessTree: true);
                leftover.WaitForExit(5000);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or Win32Exception)
            {
            }
        }
    }

    private static List<int> FindBrowserProcessIds(string userData)
    {
        using var searcher = new ManagementObjectSearcher(
            "SELECT ProcessId, CommandLine FROM Win32_Process WHERE Name = 'msedge.exe' OR Name = 'chrome.exe'");
        using var results = searcher.Get();
        var processIds = new List<int>();
        foreach (var item in results)
        {
            using (item)
            {
                if (item["CommandLine"] is string commandLine &&
                    commandLine.Contains(userData, StringComparison.OrdinalIgnoreCase))
                {
                    processIds.Add((int)(uint)item["ProcessId"]);
                }
            }
        }

        return processIds;
    }

    private static void DeleteDirectoryWithRetry(string path)
    {
        for (var attempt = 0; attempt < 10 && Directory.Exists(path); attempt++)
        {
            try
            {
                Directory.Delete(path, recursive: true);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                Thread.Sleep(300);
            }
        }
    }

    private static string FindBrowser()
    {
        var candidates = new[]
        {
            Environment.GetEnvironmentVariable("PCV_BROWSER_QA_CHROME"),
            @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
        };

        var found = candidates.FirstOrDefault(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path));
        Assert.False(string.IsNullOrWhiteSpace(found), "Edge or Chrome is required for the loopback bootstrap browser gate.");
        return found!;
    }

    private static string FindWebRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "web");
            if (File.Exists(Path.Combine(candidate, "index.html")) &&
                File.Exists(Path.Combine(candidate, "app.js")))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException("Could not locate the tracked web/ asset root from the test output directory.");
    }

    private static Process StartBrowser(string browser, int debugPort, string userData)
    {
        var start = new ProcessStartInfo
        {
            FileName = browser,
            Arguments = string.Join(' ',
            [
                "--headless=new",
                "--disable-gpu",
                "--disable-extensions",
                "--no-first-run",
                "--no-default-browser-check",
                $"--remote-debugging-port={debugPort}",
                $"--user-data-dir=\"{userData}\"",
                "about:blank"
            ]),
            UseShellExecute = false,
            CreateNoWindow = true
        };
        var process = Process.Start(start);
        Assert.NotNull(process);
        return process!;
    }

    // Deadlines only bound a hang: the test checks that the bootstrap completes, not how fast. A GitHub runner needed more
    // than 25 seconds to load the console (backlog BL-0009).
    private static readonly TimeSpan DevToolsDeadline = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan BootstrapDeadline = TimeSpan.FromSeconds(90);

    private static async Task WaitForDevToolsAsync(int debugPort)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow.Add(DevToolsDeadline);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var response = await client.GetAsync($"http://127.0.0.1:{debugPort}/json/version");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException)
            {
            }

            await Task.Delay(150);
        }

        throw new TimeoutException($"DevTools did not open on 127.0.0.1:{debugPort}.");
    }

    private static async Task<BrowserSnapshot> OpenAndReadAsync(Uri webBaseUri, int debugPort)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        using var created = await http.PutAsync(
            $"http://127.0.0.1:{debugPort}/json/new?{Uri.EscapeDataString(webBaseUri.AbsoluteUri)}",
            null);
        created.EnsureSuccessStatusCode();
        using var createdDocument = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var debuggerUrl = createdDocument.RootElement.GetProperty("webSocketDebuggerUrl").GetString();
        Assert.False(string.IsNullOrWhiteSpace(debuggerUrl));

        using var webSocket = new ClientWebSocket();
        await webSocket.ConnectAsync(new Uri(debuggerUrl!), CancellationToken.None);
        var deadline = DateTime.UtcNow.Add(BootstrapDeadline);
        BrowserSnapshot? last = null;
        while (DateTime.UtcNow < deadline)
        {
            last = await EvaluateSnapshotAsync(webSocket);
            if (!string.IsNullOrWhiteSpace(last.Session) &&
                last.Session.Contains("access_token", StringComparison.Ordinal) &&
                !last.AuthGate)
            {
                await EvaluateAsync(webSocket, "window.location.hash = '#vms'; window.dispatchEvent(new Event('hashchange')); true;");
                // BL-0019: the Single Edge nav must leave the dashboard and keep the VM section shown after the legacy re-render.
                var switched = await EvaluateSnapshotAsync(webSocket);
                for (var attempt = 0; attempt < 20 && switched.ShownView != "vms"; attempt++)
                {
                    await Task.Delay(150);
                    switched = await EvaluateSnapshotAsync(webSocket);
                }

                return switched;
            }

            await Task.Delay(200);
        }

        throw new TimeoutException(
            $"Loopback console did not complete bootstrap. last={JsonSerializer.Serialize(last)}");
    }

    private static async Task<BrowserSnapshot> EvaluateSnapshotAsync(ClientWebSocket webSocket)
    {
        var value = await EvaluateAsync(
            webSocket,
            """
            (() => ({
              connection: document.querySelector('#connection-state')?.textContent || '',
              statusVm: document.querySelector('#status-vm-count')?.textContent || '',
              authGate: Boolean(document.querySelector('[data-auth-gate]')),
              shownView: [...document.querySelectorAll('#cb .app-view')].filter(section => !section.hidden).map(section => section.dataset.view).join(','),
              session: sessionStorage.getItem('pcvDesktopAccountSession.v1') || '',
              bodyText: document.body?.innerText || ''
            }))()
            """);
        return JsonSerializer.Deserialize<BrowserSnapshot>(value, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new BrowserSnapshot();
    }

    private static async Task<string> EvaluateAsync(ClientWebSocket webSocket, string expression)
    {
        var id = Random.Shared.Next(1, int.MaxValue);
        var payload = JsonSerializer.Serialize(new
        {
            id,
            method = "Runtime.evaluate",
            @params = new
            {
                expression,
                awaitPromise = true,
                returnByValue = true
            }
        });
        await webSocket.SendAsync(
            Encoding.UTF8.GetBytes(payload),
            WebSocketMessageType.Text,
            endOfMessage: true,
            CancellationToken.None);

        var buffer = new byte[64 * 1024];
        while (true)
        {
            using var memory = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                memory.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            using var document = JsonDocument.Parse(memory.ToArray());
            if (!document.RootElement.TryGetProperty("id", out var idElement) ||
                idElement.GetInt32() != id)
            {
                continue;
            }

            if (document.RootElement.TryGetProperty("error", out var error))
            {
                throw new InvalidOperationException(error.GetRawText());
            }

            var resultElement = document.RootElement.GetProperty("result").GetProperty("result");
            if (resultElement.TryGetProperty("value", out var value) &&
                value.ValueKind == JsonValueKind.String)
            {
                return value.GetString() ?? "";
            }

            return resultElement.TryGetProperty("value", out var objectValue)
                ? objectValue.GetRawText()
                : "null";
        }
    }

    private static string WriteServiceTokenFile()
    {
        var path = Path.Combine(Path.GetTempPath(), "pcv-browser-gate-token-" + Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllText(path, ServiceTokenValue);
        return path;
    }

    private static int GetFreeLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }

    private sealed class BrowserSnapshot
    {
        public string Connection { get; set; } = "";
        public string StatusVm { get; set; } = "";
        public bool AuthGate { get; set; }
        public string ShownView { get; set; } = "";
        public string Session { get; set; } = "";
        public string BodyText { get; set; } = "";
    }
}
