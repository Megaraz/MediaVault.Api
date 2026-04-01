using media_vault_app.Application.DTOs.ExternalAPIs;
using media_vault_app.Application.DTOs.Tmdb;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services.API
{
    public class TmdbApiService : ITmdbApiService
    {
        private readonly ITmdbApiClient _client;

        public TmdbApiService(ITmdbApiClient client)
        {
            _client = client;
        }

        public async Task<Result<SearchResultDto>> GetByIdAsync(int id, MediaEntryType mediaType, CancellationToken cancellationToken = default)
        {
            var idValidationErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

            if (!id.IsValidId(idValidationErrorContext, out var idError))
            {
                idValidationErrorContext.DescriptionSuffix = $"Invalid ID: {id}.";
                return Result<SearchResultDto>.ValidationFailure([idError], idValidationErrorContext.DescriptionSuffix);
            }

            var result = await _client.GetByIdAsync(id, mediaType, cancellationToken);
            return result.Map(r => MapToSearchResult(r, mediaType));
        }

        public async Task<Result<IReadOnlyList<SearchResultDto>>> SearchAsync(
            string search,
            MediaEntryType mediaType,
            int page = 1,
            int pageSize = 10,
            string? ordering = null,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchAsync), OperationType.GetCollection);
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
                return Result<IReadOnlyList<SearchResultDto>>.ValidationFailure(errors, "TMDB search validation failed.");
            }

            var queryParameters = new List<string>
            {
                $"query={Uri.EscapeDataString(search)}",
                $"page={page}"
            };

            var result = await _client.SearchAsync(queryParameters, mediaType, cancellationToken);

            return result.Map(searchResponse => MapToSearchResults(searchResponse.Results, mediaType));
        }

        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? entityName = null, string? fieldName = null)
        {
            return new ErrorContext(
                layer: "Application",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: entityName ?? "Tmdb",
                fieldName: fieldName);
        }

        private static IReadOnlyList<SearchResultDto> MapToSearchResults(IReadOnlyList<TmdbResult>? results, MediaEntryType mediaType)
        {
            return results?.Select(r => MapToSearchResult(r, mediaType)).ToArray() ?? Array.Empty<SearchResultDto>();
        }

        private static SearchResultDto MapToSearchResult(TmdbResult result, MediaEntryType mediaType)
        {
            return new SearchResultDto(
                result.Id.ToString(),
                result.Title ?? result.Name ?? string.Empty,
                BuildImageUrl(result.PosterPath),
                mediaType);
        }

        private static string? BuildImageUrl(string? path)
        {
            const string tmdbImageBaseUrl = "https://image.tmdb.org/t/p/w500";
            return string.IsNullOrEmpty(path) ? null : $"{tmdbImageBaseUrl}{path}";
        }
    }
}
