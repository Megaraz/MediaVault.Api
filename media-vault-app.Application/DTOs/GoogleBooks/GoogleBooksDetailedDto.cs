using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.GoogleBooks
{
    public sealed record GoogleBooksDetailedDto(
        string Author,
        string ExternalId,
        string Title,
        string? CoverImageUrl,
        MediaType MediaType
        );


}