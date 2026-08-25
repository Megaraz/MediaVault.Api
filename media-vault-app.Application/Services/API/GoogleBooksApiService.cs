using media_vault_app.Application.DTOs.External_API_Contracts.GoogleBooks;
using media_vault_app.Application.DTOs.GoogleBooks;
using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Domain.Enums;
using Microsoft.Extensions.Logging;
using media_vault_app.Application.Pagination;
using Megaraz.ResultPattern;
using media_vault_app.Application.Validation;

namespace media_vault_app.Application.Services.API
{
    public class GoogleBooksApiService : IGoogleBooksApiService
    {
        private readonly IGoogleBooksApiClient _client;
        private readonly ILogger<GoogleBooksApiService> _logger;

        public GoogleBooksApiService(IGoogleBooksApiClient client, ILogger<GoogleBooksApiService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<Result<GoogleBooksDetailedDto>> GetBookByIdAsync(string volumeId, CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(GetBookByIdAsync), OperationType.Get);

            if (string.IsNullOrWhiteSpace(volumeId))
            {
                var error = MediaVaultValidationError.Required(errorContext with { FieldName = nameof(volumeId) });
                ServiceValidationLogging.LogValidationFailure(
                    _logger, [error], nameof(GoogleBooksApiService), nameof(GetBookByIdAsync), errorContext);
                return Result<GoogleBooksDetailedDto>.ValidationFailure([error], error.UserMessage);
            }

            var clientResult = await _client.GetBookByIdAsync(volumeId, cancellationToken);

            return clientResult.Map(ToDetailedDto);
        }

        public async Task<Result<IReadOnlyList<GoogleBooksDetailedDto>>> SearchBooksAsync(
            string search,
            int page = 1,
            int pageSize = 8,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchBooksAsync), OperationType.GetCollection);
            List<ValidationError> errors = new();

            if (search.IsMissingMediaVaultValue(errorContext with { FieldName = nameof(search) }, out var searchError))
            {
                errors.Add(searchError);
            }

            var pagination = PaginationParameters.Normalize(page, pageSize);
            page = pagination.PageNumber;
            pageSize = pagination.PageSize;

            if (errors.Count > 0)
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, errors, nameof(GoogleBooksApiService), nameof(SearchBooksAsync), errorContext);
                return Result<IReadOnlyList<GoogleBooksDetailedDto>>.ValidationFailure(errors, "Google Books search validation failed.");
            }

            var startIndex = (page - 1) * pageSize;

            var queryParameters = new List<string>
            {
                $"q={Uri.EscapeDataString(search)}",
                $"startIndex={startIndex}",
                $"maxResults={pageSize}"
            };

            var clientResult = await _client.SearchBooksAsync(queryParameters, cancellationToken);

            return clientResult.Map(searchResponse => ToDetailedDtoCollection(searchResponse.Items));
        }

        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? entityName = null, string? fieldName = null)
        {
            return new ErrorContext(
                operation: operation,
                entityName: entityName ?? "Google Books Volume",
                fieldName: fieldName);
        }

        private static IReadOnlyList<MediaEntryExternalSearchResultDto> MapToSearchResults(IReadOnlyList<GoogleBooksVolumeResponse>? volumes)
        {
            return volumes?.Select(MapToSearchResult).ToArray() ?? Array.Empty<MediaEntryExternalSearchResultDto>();
        }

        private static IReadOnlyList<GoogleBooksDetailedDto> ToDetailedDtoCollection(IReadOnlyList<GoogleBooksVolumeResponse>? volumes)
        {
            return volumes?.Select(ToDetailedDto).ToArray() ?? Array.Empty<GoogleBooksDetailedDto>();
        }

        private static GoogleBooksDetailedDto ToDetailedDto(GoogleBooksVolumeResponse volume)
        {
            var thumbnailUrl = volume.VolumeInfo?.ImageLinks is null
                ? null
                : volume.VolumeInfo.ImageLinks.Small
                    ?? volume.VolumeInfo.ImageLinks.Thumbnail
                    ?? volume.VolumeInfo.ImageLinks.Medium
                    ?? volume.VolumeInfo.ImageLinks.SmallThumbnail
                    ?? volume.VolumeInfo.ImageLinks.Large
                    ?? volume.VolumeInfo.ImageLinks.ExtraLarge
                    ?? null;
            // Google Books may return http:// URLs — upgrade to https://
            if (thumbnailUrl != null && thumbnailUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                thumbnailUrl = string.Concat("https://", thumbnailUrl.AsSpan("http://".Length));
            }
            return new GoogleBooksDetailedDto(
                Author: volume.VolumeInfo?.Authors != null && volume.VolumeInfo.Authors.Any()
                    ? string.Join(", ", volume.VolumeInfo.Authors)
                    : "Unknown Author",
                ExternalId: volume.Id,
                Title: volume.VolumeInfo?.Title ?? string.Empty,
                CoverImageUrl: thumbnailUrl,
                MediaType: MediaType.Book);
        }

        private static MediaEntryExternalSearchResultDto MapToSearchResult(GoogleBooksVolumeResponse volume)
        {
            var thumbnailUrl = volume.VolumeInfo?.ImageLinks is null
                ? null
                : volume.VolumeInfo.ImageLinks.Small
                    ?? volume.VolumeInfo.ImageLinks.Thumbnail
                    ?? volume.VolumeInfo.ImageLinks.Medium
                    ?? volume.VolumeInfo.ImageLinks.SmallThumbnail
                    ?? volume.VolumeInfo.ImageLinks.Large
                    ?? volume.VolumeInfo.ImageLinks.ExtraLarge
                    ?? null;

            // Google Books may return http:// URLs — upgrade to https://
            if (thumbnailUrl != null && thumbnailUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                thumbnailUrl = string.Concat("https://", thumbnailUrl.AsSpan("http://".Length));
            }

            return new MediaEntryExternalSearchResultDto(
                volume.Id,
                volume.VolumeInfo?.Title ?? string.Empty,
                thumbnailUrl,
                MediaType.Book);
        }
    }
}
