using System.Diagnostics.CodeAnalysis;
using Amazon.Lambda.AspNetCoreServer;

namespace LambdaApiReference;

/// <summary>
/// Entry point invoked by AWS Lambda when the function is triggered via an
/// API Gateway REST API (v1) proxy event.
///
/// The base class <see cref="APIGatewayProxyFunction"/> handles translating the
/// Lambda proxy event into a standard ASP.NET Core HttpContext and back.
///
/// Lambda handler (set in serverless.yml / aws-lambda-tools-defaults.json):
///   LambdaApiReference::LambdaApiReference.LambdaEntryPoint::FunctionHandlerAsync
/// </summary>
[ExcludeFromCodeCoverage]
public class LambdaEntryPoint : APIGatewayProxyFunction
{
    /// <summary>
    /// Configure the web host the same way as <see cref="LocalEntryPoint"/>.
    /// </summary>
    protected override void Init(IWebHostBuilder builder)
    {
        builder.UseStartup<Startup>();
    }
}
