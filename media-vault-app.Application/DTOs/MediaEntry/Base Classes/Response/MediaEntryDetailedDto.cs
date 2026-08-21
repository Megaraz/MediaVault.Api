using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Application.DTOs.MediaEntry.Response;

public abstract record MediaEntryDetailedDto : IDtoIdentifiable<Guid>
{
    public Guid Id { get; init; }
    public string? IdExternal { get; init; }
    public Guid UserId { get; init; }
    public Status Status { get; init; }
    public required string Title { get; init; }
    public decimal Rating { get; init; }
    public string? Overview { get; init; }
    public string? Review { get; init; }
    public ICollection<string> Genres { get; init; } = new List<string>();
    public DateOnly ReleaseDate { get; init; }
    public string? ImageUrl { get; init; }
    public abstract MediaType MediaType { get; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
    public int Version { get; init; }
}
