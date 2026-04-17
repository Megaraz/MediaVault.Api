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
                return Result<SearchResultDto>.ValidationFailure([idError]);
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
            var baseErrorContext = DefineErrorContext(nameof(SearchAsync), OperationType.GetCollection);

            List<ValidationError> errors = new();

            if (search.IsNullOrWhiteSpace(baseErrorContext with { FieldName = nameof(search) }, out var searchError))
            {
                errors.Add(searchError);
            }

            Validator.ValidateAndAdjustPaginationParameters(ref page, ref pageSize);

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
                Layer: "Application",
                ServiceName: GetType().Name,
                MethodName: methodName,
                Operation: operation,
                EntityName: entityName ?? "Tmdb",
                FieldName: fieldName);
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
