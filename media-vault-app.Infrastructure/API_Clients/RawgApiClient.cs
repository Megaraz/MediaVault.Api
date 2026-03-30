using System.Text.Json.Serialization;
using media_vault_app.Application.DTOs.Rawg;
using media_vault_app.Application.Interfaces.Clients;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.API_Clients
{
    public sealed record RawgApiOptions(string BaseUrl, string ApiKey);


    public sealed class RawgApiClient : IRawgApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly RawgApiOptions _options;

        public RawgApiClient(HttpClient httpClient, RawgApiOptions options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<Result<RawgGameResponse>> GetGameAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(BuildRequestUri($"games/{id}"), cancellationToken);

            var httpResponseErrorContext = DefineErrorContext(nameof(GetGameAsync), OperationType.Get, fieldName: $"{id}");


            return await response.MapAsync<RawgGameResponse>(httpResponseErrorContext, cancellationToken);

            //var result = await response.MapAsync<RawgGameResponse>(httpResponseErrorContext, cancellationToken);

            //return result.Map(MapToGameSearchResult);
        }

        public async Task<Result<IReadOnlyList<RawgSearchResponse>>> SearchGamesAsync(
            List<string> queryParameters,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchGamesAsync), OperationType.GetCollection);

            var requestUri = BuildRequestUri($"games?{string.Join("&", queryParameters)}");

            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            return await response.MapAsync<IReadOnlyList<RawgSearchResponse>>(errorContext, cancellationToken);
        }

        private string BuildRequestUri(string pathAndQuery)
        {
            var separator = pathAndQuery.Contains('?') ? "&" : "?";
            return $"{pathAndQuery}{separator}key={Uri.EscapeDataString(_options.ApiKey)}";
        }


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
