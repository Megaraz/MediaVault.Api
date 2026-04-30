using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Enums;

namespace media_vault_app.Application.DTOs.GoogleBooks
{
    public sealed record GoogleBooksSearchResultDto : SearchResultDto
    {
        public string Author { get; init; } = string.Empty;

        public GoogleBooksSearchResultDto(
            string externalId,
            string title,
            string? coverImageUrl,
            string author,
            MediaType mediaType
            ) : base(externalId, title, coverImageUrl, mediaType)
        {
            Author = author;
        }
    }
}