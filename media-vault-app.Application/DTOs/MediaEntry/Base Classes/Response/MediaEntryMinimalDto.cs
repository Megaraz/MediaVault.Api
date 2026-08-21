using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Application.DTOs.MediaEntry.Response;

public record MediaEntryMinimalDto : IDtoIdentifiable<Guid>
{
    public Guid Id { get; init; }
    public string? Title { get; init; }
    public Status Status { get; init; }
    public ICollection<string> Genres { get; init; } = new List<string>();
    public DateOnly ReleaseDate { get; init; }
    public MediaType MediaType { get; init; }
    public decimal Rating { get; init; }
    public string? ImageUrl { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
    public int Version { get; init; }
}
