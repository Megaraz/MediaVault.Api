using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using media_vault_app.Application.DTOs.Rawg;
using media_vault_app.Application.Interfaces.Clients;
using Microsoft.Extensions.Options;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.API_Clients
{
    public sealed class RawgApiOptions
    {
        public const string SectionName = "ExternalApis:Rawg";

        [Required]
        public string BaseUrl { get; set; } = string.Empty;

        [Required]
        public string ApiKey { get; set; } = string.Empty;
    }


    public sealed class RawgApiClient : IRawgApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly RawgApiOptions _options;

        public RawgApiClient(HttpClient httpClient, IOptions<RawgApiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<Result<RawgGameResponse>> GetGameAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(BuildRequestUri($"games/{id}"), cancellationToken);

            var httpResponseErrorContext = DefineErrorContext(nameof(GetGameAsync), OperationType.Get, fieldName: $"{id}");


            return await response.MapAsync<RawgGameResponse>(httpResponseErrorContext, cancellationToken);
        }

        public async Task<Result<RawgSearchResponse>> SearchGamesAsync(
            List<string> queryParameters,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchGamesAsync), OperationType.GetCollection);

            var requestUri = BuildRequestUri($"games?{string.Join("&", queryParameters)}");

            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            return await response.MapAsync<RawgSearchResponse>(errorContext, cancellationToken);
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
