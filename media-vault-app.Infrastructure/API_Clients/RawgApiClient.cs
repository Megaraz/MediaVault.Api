using System.Text.Json.Serialization;
using media_vault_app.Application.DTOs.Rawg;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.API_Clients
{
    public sealed record RawgApiOptions(string BaseUrl, string ApiKey);

    public sealed class RawgApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly RawgApiOptions _options;
        private readonly HttpResponseResultMapper _httpResponseResultMapper;

        public RawgApiClient(HttpClient httpClient, RawgApiOptions options)
        {
            _httpClient = httpClient;
            _options = options;
            _httpResponseResultMapper = new HttpResponseResultMapper();
        }

        public async Task<Result<GameSearchResultDto>> GetGameAsync(int id, CancellationToken cancellationToken = default)
        {
            var idValidationErrorContext = DefineErrorContext(nameof(GetGameAsync), OperationType.Get);

            if (!id.IsValidId(idValidationErrorContext, out var idError))
            {
                idValidationErrorContext.DescriptionSuffix = $"Invalid game ID: {id}.";
                return Result<GameSearchResultDto>.ValidationFailure([idError], idValidationErrorContext.DescriptionSuffix);
            }

            using var response = await _httpClient.GetAsync(BuildRequestUri($"games/{id}"), cancellationToken);

            var httpResponseErrorContext = DefineErrorContext(nameof(GetGameAsync), OperationType.Get, fieldName: $"{id}");
            var result = await _httpResponseResultMapper.FromResponseWithValue<RawgGameResponse>(response, httpResponseErrorContext, cancellationToken);

            return result.Map(MapToGameSearchResult);
        }

        public async Task<Result<IReadOnlyList<GameSearchResultDto>>> SearchGamesAsync(
            string search,
            int page = 1,
            int pageSize = 10,
            bool? searchPrecise = null,
            bool? searchExact = null,
            string? ordering = null,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchGamesAsync), OperationType.GetCollection);
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
                return Result<IReadOnlyList<GameSearchResultDto>>.ValidationFailure(errors, "RAWG game search validation failed.");
            }

            var queryParameters = new List<string>
            {
                $"search={Uri.EscapeDataString(search)}",
                $"page={page}",
                $"page_size={pageSize}"
            };

            if (searchPrecise.HasValue)
            {
                queryParameters.Add($"search_precise={searchPrecise.Value.ToString().ToLowerInvariant()}");
            }

            if (searchExact.HasValue)
            {
                queryParameters.Add($"search_exact={searchExact.Value.ToString().ToLowerInvariant()}");
            }

            if (!string.IsNullOrWhiteSpace(ordering))
            {
                queryParameters.Add($"ordering={Uri.EscapeDataString(ordering)}");
            }

            var requestUri = BuildRequestUri($"games?{string.Join("&", queryParameters)}");

            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            var result = await _httpResponseResultMapper.FromResponseWithValue<RawgSearchResponse>(response, errorContext, cancellationToken);

            return result.Map(searchResponse =>
                (IReadOnlyList<GameSearchResultDto>)(searchResponse.Results?.Select(MapToGameSearchResult).ToArray()
                ?? Array.Empty<GameSearchResultDto>()));
        }

        private string BuildRequestUri(string pathAndQuery)
        {
            var separator = pathAndQuery.Contains('?') ? "&" : "?";
            return $"{pathAndQuery}{separator}key={Uri.EscapeDataString(_options.ApiKey)}";
        }

        private static GameSearchResultDto MapToGameSearchResult(RawgGameResponse rawgGame)
        {
            return new GameSearchResultDto(
                rawgGame.Id,
                rawgGame.Name ?? string.Empty,
                rawgGame.BackgroundImage,
                rawgGame.Slug ?? string.Empty);
        }

        private sealed record RawgSearchResponse(
            [property: JsonPropertyName("results")] IReadOnlyList<RawgGameResponse>? Results);

        private sealed record RawgGameResponse(
            [property: JsonPropertyName("id")] int Id,
            [property: JsonPropertyName("slug")] string? Slug,
            [property: JsonPropertyName("name")] string? Name,
            [property: JsonPropertyName("background_image")] string? BackgroundImage);

        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? entityName = null, string? fieldName = null)
        {
            return new ErrorContext(
                layer: "Infrastructure",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: entityName ?? "Rawg Game",
                fieldName: fieldName);
        }

    }


}
