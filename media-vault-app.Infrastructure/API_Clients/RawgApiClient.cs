using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using media_vault_app.Application.DTOs.Rawg;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.API_Clients
{
    public sealed record RawgApiOptions(string BaseUrl, string ApiKey)
    {
        public string NormalizedBaseUrl => NormalizeBaseUrl(BaseUrl);

        public static string NormalizeBaseUrl(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("RAWG base URL is not configured.");
            }

            var normalizedBaseUrl = baseUrl.Trim();
            var gamesSegmentIndex = normalizedBaseUrl.IndexOf("/games", StringComparison.OrdinalIgnoreCase);

            if (gamesSegmentIndex >= 0)
            {
                normalizedBaseUrl = normalizedBaseUrl[..gamesSegmentIndex];
            }

            var queryIndex = normalizedBaseUrl.IndexOf('?');

            if (queryIndex >= 0)
            {
                normalizedBaseUrl = normalizedBaseUrl[..queryIndex];
            }

            if (!normalizedBaseUrl.EndsWith('/'))
            {
                normalizedBaseUrl += "/";
            }

            return normalizedBaseUrl;
        }
    }

    public sealed class RawgApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly RawgApiOptions _options;

        public RawgApiClient(HttpClient httpClient, RawgApiOptions options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<Result<GameSearchResultDto>> GetGameAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(BuildRequestUri($"games/{id}"), cancellationToken);

            var errorContext = DefineErrorContext(nameof(GetGameAsync), OperationType.Get);

            if (!id.IsValidId(errorContext, out var idError))
            {
                errorContext.DescriptionSuffix = $"Invalid game ID: {id}.";
                return Result<GameSearchResultDto>.ValidationFailure([idError], errorContext.DescriptionSuffix);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Result<GameSearchResultDto>.Failure(Error.NotFound(errorContext), $"Game with ID {id} not found in RAWG.");
            }

            response.EnsureSuccessStatusCode();

            var rawgGame = await response.Content.ReadFromJsonAsync<RawgGameResponse>(cancellationToken: cancellationToken);

            return rawgGame is null
                ? Result<GameSearchResultDto>.Failure(Error.NotFound(errorContext), $"Game with ID {id} not found in RAWG.")
                : Result<GameSearchResultDto>.Success(MapToGameSearchResult(rawgGame));
        }

        public async Task<IReadOnlyList<GameSearchResultDto>> SearchGamesAsync(
            string search,
            int page = 1,
            int pageSize = 10,
            bool? searchPrecise = null,
            bool? searchExact = null,
            string? ordering = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return Array.Empty<GameSearchResultDto>();
            }

            var queryParameters = new List<string>
            {
                $"search={Uri.EscapeDataString(search)}",
                $"page={Math.Max(page, 1)}",
                $"page_size={Math.Max(pageSize, 1)}"
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
            response.EnsureSuccessStatusCode();

            var searchResponse = await response.Content.ReadFromJsonAsync<RawgSearchResponse>(cancellationToken: cancellationToken);

            return searchResponse?.Results?.Select(MapToGameSearchResult).ToArray()
                ?? Array.Empty<GameSearchResultDto>();
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

        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? entityName = null)
        {
            return new ErrorContext(
                layer: "Infrastructure",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: "Rawg");
        }

    }


}
