using System.Diagnostics.CodeAnalysis;

namespace LambdaApiReference.Models;

[ExcludeFromCodeCoverage]
public record ReferenceOptions
{
    public string Region { get; set; } = string.Empty;
    public string ReferencePublishSnsTopic { get; set; } = string.Empty;
}
