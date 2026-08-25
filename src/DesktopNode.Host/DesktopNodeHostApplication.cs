using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using DesktopNode.Api;

namespace DesktopNode.Host;

public sealed partial class DesktopNodeHostApplication : IDisposable
{
    private sealed class DesktopNodeHostRequestBodyTooLargeException(long maxBytes)
        : Exception($"Request body exceeds {maxBytes} bytes.")
    {
        public long MaxBytes { get; } = maxBytes;
    }

    private sealed record ListenerBinding(
        HttpListener Listener,
        Uri BaseUri,
        bool ServesApi,
        bool ServesStatic,
        bool IsLoopback);

    private readonly IReadOnlyList<ListenerBinding> listeners;
    private readonly CancellationTokenSource cancellation = new();
    private readonly IReadOnlyList<Task> loopTasks;
    private readonly DesktopNodeHostOptions options;
    private readonly DesktopNodeApiRequestProcessor processor;
    private readonly DesktopNodeHostResolvedToken token;
    private readonly string? allowedWebOrigin;
    private readonly bool accountAuthReady;
    private readonly DesktopNodeAccountAuthService accountAuthService;
    private readonly DesktopNodeRequestAdmission? requestAdmission;
    private readonly ConcurrentDictionary<int, Task> requestTasks = new();
    private int nextRequestId;
    private int disposed;

    private DesktopNodeHostApplication(
        IReadOnlyList<ListenerBinding> listeners,
        Uri apiBaseUri,
        Uri webBaseUri,
        DesktopNodeHostOptions options,
        DesktopNodeApiRequestProcessor processor,
        DesktopNodeHostResolvedToken token,
        string? allowedWebOrigin,
        bool accountAuthReady,
        DesktopNodeAccountAuthService accountAuthService)
    {
        this.listeners = listeners;
        this.options = options;
        this.processor = processor;
        this.token = token;
        this.allowedWebOrigin = allowedWebOrigin;
        this.accountAuthReady = accountAuthReady;
        this.accountAuthService = accountAuthService;
        requestAdmission = options.RequestLifetimeMode == DesktopNodeRequestLifetimeMode.TrackedAsyncSerialized
            ? new DesktopNodeRequestAdmission(
                options.RequestAdmissionActiveLimit,
                options.RequestAdmissionWaitingLimit)
            : null;
        BaseUri = apiBaseUri;
        WebBaseUri = webBaseUri;
        var listenerTasks = listeners
            .Select(binding => Task.Run(() => RunAsync(binding, cancellation.Token)))
            .ToArray();
        var workerTask = Task.Run(
            () => processor.RunWorkerLoopAsync(cancellation.Token, workerCount: 1),
            cancellation.Token);
        loopTasks = listenerTasks.Append(workerTask).ToArray();
    }

    public Uri BaseUri { get; }
    public Uri WebBaseUri { get; }

    internal int RequestAdmissionActiveCount => requestAdmission?.ActiveCount ?? 0;

    public static Task<DesktopNodeHostApplication> StartAsync(DesktopNodeHostOptions options)
    {
        DesktopNodeHostJobRuntimeEventSink.ValidatePathSeparation(options);
        var apiPrefix = NormalizePrefix(options.Prefix);
        var apiBaseUri = new Uri(apiPrefix);
        var webPrefix = string.IsNullOrWhiteSpace(options.WebPrefix)
            ? null
            : NormalizePrefix(options.WebPrefix);
        var hasSeparateWebPrefix = !string.IsNullOrWhiteSpace(webPrefix) &&
            !string.Equals(apiPrefix, webPrefix, StringComparison.OrdinalIgnoreCase);
        var webBaseUri = hasSeparateWebPrefix ? new Uri(webPrefix!) : apiBaseUri;
        var bindings = new List<ListenerBinding>();
        try
        {
            bindings.Add(StartListener(
                apiPrefix,
                servesApi: true,
                servesStatic: !hasSeparateWebPrefix && !string.IsNullOrWhiteSpace(options.WebRootPath)));
            if (hasSeparateWebPrefix)
            {
                bindings.Add(StartListener(
                    webPrefix!,
                    servesApi: false,
                    servesStatic: !string.IsNullOrWhiteSpace(options.WebRootPath)));
            }

            var token = DesktopNodeHostTokenResolver.Resolve(options);
            var apiIsLoopback = IsLoopbackPrefix(apiPrefix);
            var allowedWebOrigin = hasSeparateWebPrefix
                ? webBaseUri.GetLeftPart(UriPartial.Authority)
                : null;
            var accountAuthOptions = options.AccountAuthOptions ??
                DesktopNodeAccountAuthOptions.FromFiles(options.AccountFilePath, options.JwtSigningKeyFilePath);
            var accountAuthService = new DesktopNodeAccountAuthService(accountAuthOptions);
            var consoleOptions = new DesktopNodeConsoleOptions(
                NoVncEnabled: options.NoVncBridgeEnabled,
                NoVncWebSocketPath: options.NoVncWebSocketPath,
                NoVncBridgeMode: options.NoVncBridgeEnabled ? "websocket-to-vnc-tcp" : "disabled");

            return Task.FromResult(new DesktopNodeHostApplication(
                bindings,
                apiBaseUri,
                webBaseUri,
                options,
                DesktopNodeApiRequestProcessor.CreateDefault(
                    token.Storage,
                    apiIsLoopback ? "loopback" : "lan",
                    jobStorePath: options.JobStorePath,
                    batchEvidenceRoot: options.BatchEvidenceRootPath,
                    hardeningOptions: new DesktopNodeApiHardeningOptions(
                        RouteTimeoutSeconds: options.RouteTimeoutSeconds,
                        RequestLimitPerMinute: options.RequestLimitPerMinute,
                        BurstLimit: options.RequestBurstLimit,
                        RetryAfterSeconds: options.RetryAfterSeconds,
                        ControlledRouteTimeoutProbeDelayMilliseconds: options.ControlledRouteTimeoutProbeDelayMilliseconds),
                    diagnosticBundleOptions: new DesktopNodeDiagnosticBundleOptions(
                        options.DiagnosticsRootPath),
                    accountAuthOptions: accountAuthOptions,
                    consoleOptions: consoleOptions,
                    jobRuntimeEventSink: new DesktopNodeHostJobRuntimeEventSink(options)),
                token,
                allowedWebOrigin,
                accountAuthOptions.Ready,
                accountAuthService));
        }
        catch
        {
            foreach (var binding in bindings)
            {
                try
                {
                    binding.Listener.Stop();
                }
                catch (ObjectDisposedException)
                {
                }

                binding.Listener.Close();
            }

            throw;
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        cancellation.Cancel();
        foreach (var binding in listeners)
        {
            if (binding.Listener.IsListening)
            {
                binding.Listener.Stop();
            }

            binding.Listener.Close();
        }

        try
        {
            Task.WaitAll(loopTasks.ToArray(), TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
        }

        var requestSnapshot = requestTasks.Values.ToArray();
        try
        {
            Task.WaitAll(requestSnapshot, TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
        }

        requestAdmission?.Dispose();
        cancellation.Dispose();
    }

}
