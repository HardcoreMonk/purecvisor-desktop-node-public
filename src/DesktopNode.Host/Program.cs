using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DesktopNode.Host;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var options = DesktopNodeHostOptions.Parse(args);
            if (options.Mode == DesktopNodeHostMode.ServiceAction)
            {
                var result = await DesktopNodeHostServiceAction.ExecuteAsync(options).ConfigureAwait(false);
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
                return result.Ok ? 0 : 1;
            }

            var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
            builder.Services.AddSingleton(options);
            builder.Services.AddHostedService<DesktopNodeWindowsService>();
            builder.Services.AddWindowsService(serviceOptions =>
            {
                serviceOptions.ServiceName = "PureCVisorDesktopNode";
            });

            using var host = builder.Build();
            await host.RunAsync().ConfigureAwait(false);
            return 0;
        }
        catch (ArgumentException error)
        {
            Console.Error.WriteLine(error.Message);
            return 2;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"PCV_HOST_RUNTIME_FAILED|The .NET Desktop Node host failed.|{error.Message}");
            return 1;
        }
    }
}
