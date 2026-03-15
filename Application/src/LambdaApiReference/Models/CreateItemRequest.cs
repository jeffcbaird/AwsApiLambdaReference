using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace LambdaApiReference.Models;

[ExcludeFromCodeCoverage]
public record CreateItemRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
    public required string Name { get; init; }

    [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
    public string? Description { get; init; }
}
