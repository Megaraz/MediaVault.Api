using media_vault_app.Application.DTOs;
using media_vault_app.Application.DTOs.External_API_Contracts.GoogleBooks;
using media_vault_app.Application.DTOs.GoogleBooks;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services.API
{
    public class GoogleBooksApiService : IGoogleBooksApiService
    {
        private readonly IGoogleBooksApiClient _client;

        public GoogleBooksApiService(IGoogleBooksApiClient client)
        {
            _client = client;
        }

        public async Task<Result<SearchResultDto>> GetBookByIdAsync(string volumeId, CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(GetBookByIdAsync), OperationType.Get);

            if (string.IsNullOrWhiteSpace(volumeId))
            {
                var volumeIdErrorContext = errorContext with { FieldName = nameof(volumeId), DescriptionSuffix = "Volume ID must not be empty." };
                var error = ValidationError.Required(volumeIdErrorContext);
                return Result<SearchResultDto>.ValidationFailure([error], volumeIdErrorContext.DescriptionSuffix);
            }

            var result = await _client.GetBookAsync(volumeId, cancellationToken);

            return result.Map(MapToSearchResult);
        }

        public async Task<Result<IReadOnlyList<SearchResultDto>>> SearchBooksAsync(
            string search,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchBooksAsync), OperationType.GetCollection);
            List<ValidationError> errors = new();

            if (search.IsNullOrWhiteSpace(errorContext with { FieldName = nameof(search) }, out var searchError))
            {
                errors.Add(searchError);
            }

            Validator.ValidateAndAdjustPaginationParameters(ref page, ref pageSize);

            if (errors.Any())
            {
                return Result<IReadOnlyList<SearchResultDto>>.ValidationFailure(errors, "Google Books search validation failed.");
            }

            var startIndex = (page - 1) * pageSize;

            var queryParameters = new List<string>
            {
                $"q={Uri.EscapeDataString(search)}",
                $"startIndex={startIndex}",
                $"maxResults={pageSize}"
            };

            var result = await _client.SearchBooksAsync(queryParameters, cancellationToken);

            return result.Map(searchResponse => MapToSearchResults(searchResponse.Items));
        }

        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? entityName = null, string? fieldName = null)
        {
            return new ErrorContext(
                Layer: "Application",
                ServiceName: GetType().Name,
                MethodName: methodName,
                Operation: operation,
                EntityName: entityName ?? "Google Books Volume",
                FieldName: fieldName);
        }

        private static IReadOnlyList<SearchResultDto> MapToSearchResults(IReadOnlyList<GoogleBooksVolumeResponse>? volumes)
        {
            return volumes?.Select(MapToSearchResult).ToArray() ?? Array.Empty<SearchResultDto>();
        }

        private static SearchResultDto MapToSearchResult(GoogleBooksVolumeResponse volume)
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
                thumbnailUrl = "https://" + thumbnailUrl.Substring("http://".Length);
            }

            return new SearchResultDto(
                volume.Id,
                volume.VolumeInfo?.Title ?? string.Empty,
                thumbnailUrl,
                MediaType.Book);
        }
    }
}
