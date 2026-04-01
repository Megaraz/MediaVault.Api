using media_vault_app.Application.DTOs.ExternalAPIs;
using media_vault_app.Application.DTOs.GoogleBooks;
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
                errorContext.FieldName = nameof(volumeId);
                errorContext.DescriptionSuffix = "Volume ID must not be empty.";
                var error = ValidationError.Required(errorContext);
                return Result<SearchResultDto>.ValidationFailure([error], errorContext.DescriptionSuffix);
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

            errorContext.FieldName = nameof(search);
            if (search.IsNullOrWhiteSpace(errorContext, out var searchError))
            {
                errors.Add(searchError);
            }

            if (page < 1)
            {
                errorContext.DescriptionSuffix = "Page number must be greater than 0.";
                errorContext.FieldName = nameof(page);

                var pageError = ValidationError.OutOfRange(errorContext, "Greater than 0");
                errors.Add(pageError);
            }

            if (pageSize < 1)
            {
                errorContext.DescriptionSuffix = "Page size must be greater than 0.";
                errorContext.FieldName = nameof(pageSize);

                var pageSizeError = ValidationError.OutOfRange(errorContext, "Greater than 0");
                errors.Add(pageSizeError);
            }

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
                layer: "Application",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: entityName ?? "Google Books Volume",
                fieldName: fieldName);
        }

        private static IReadOnlyList<SearchResultDto> MapToSearchResults(IReadOnlyList<GoogleBooksVolumeResponse>? volumes)
        {
            return volumes?.Select(MapToSearchResult).ToArray() ?? Array.Empty<SearchResultDto>();
        }

        private static SearchResultDto MapToSearchResult(GoogleBooksVolumeResponse volume)
        {
            var thumbnailUrl = volume.VolumeInfo?.ImageLinks?.Thumbnail;

            // Google Books may return http:// URLs — upgrade to https://
            if (thumbnailUrl != null && thumbnailUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                thumbnailUrl = "https://" + thumbnailUrl.Substring("http://".Length);
            }

            return new SearchResultDto(
                volume.Id,
                volume.VolumeInfo?.Title ?? string.Empty,
                thumbnailUrl,
                MediaEntryType.BookEntry);
        }
    }
}
