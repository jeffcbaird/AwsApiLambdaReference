using System.Diagnostics.CodeAnalysis;

namespace LambdaApiReference;

/// <summary>
/// Entry point used for local development via <c>dotnet run</c> or <c>make run</c>.
/// Not referenced when the function runs in AWS Lambda.
/// </summary>
[ExcludeFromCodeCoverage]
public static class LocalEntryPoint
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(web => web.UseStartup<Startup>());
}
