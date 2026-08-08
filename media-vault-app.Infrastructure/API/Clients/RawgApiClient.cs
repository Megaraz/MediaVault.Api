using System.ComponentModel.DataAnnotations;
using media_vault_app.Application.DTOs.External_API_Contracts.Rawg;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Infrastructure.Diagnostics;
using Microsoft.Extensions.Options;
using Megaraz.ResultPattern;

namespace media_vault_app.Infrastructure.API.Clients
{
    public sealed class RawgApiOptions
    {
        public const string SectionName = "ExternalApis:Rawg";

        [Required]
        public string BaseUrl { get; set; } = string.Empty;

        [Required]
        public string ApiKey { get; set; } = string.Empty;
    }


    public sealed class RawgApiClient : ApiClientBase<RawgApiClient>, IRawgApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly RawgApiOptions _options;

        public RawgApiClient(
            HttpClient httpClient,
            IOptions<RawgApiOptions> options,
            ErrorEventLogger<RawgApiClient> errorEventLogger)
            : base(errorEventLogger, "RAWG")
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<Result<RawgGameDetailedResponse>> GetGameByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(GetGameByIdAsync), OperationType.Get);

            return await SendAndMapAsync<RawgGameDetailedResponse>(
                ct => _httpClient.GetAsync(BuildRequestUri($"games/{id}"), ct),
                errorContext,
                cancellationToken);
        }

        public async Task<Result<RawgSearchResponse>> SearchGamesAsync(
            List<string> queryParameters,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchGamesAsync), OperationType.GetCollection);

            var requestUri = BuildRequestUri($"games?{string.Join("&", queryParameters)}");

            return await SendAndMapAsync<RawgSearchResponse>(
                ct => _httpClient.GetAsync(requestUri, ct),
                errorContext,
                cancellationToken);
        }

        private string BuildRequestUri(string pathAndQuery)
        {
            var separator = pathAndQuery.Contains('?') ? "&" : "?";
            return $"{pathAndQuery}{separator}key={Uri.EscapeDataString(_options.ApiKey)}";
        }


        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? entityName = null, string? fieldName = null)
        {
            return new ErrorContext(
                operation: operation,
                entityName: entityName ?? "Rawg Game",
                fieldName: fieldName);
        }

    }


}
