using System.ComponentModel.DataAnnotations;
using media_vault_app.Application.DTOs.External_API_Contracts.GoogleBooks;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Infrastructure.Diagnostics;
using Microsoft.Extensions.Options;
using Megaraz.ResultPattern;

namespace media_vault_app.Infrastructure.API.Clients
{

    public sealed class GoogleBooksApiOptions
    {
        public const string SectionName = "ExternalApis:GoogleBooks";

        [Required]
        public string BaseUrl { get; set; } = string.Empty;

        [Required]
        public string ApiKey { get; set; } = string.Empty;
    }

    public sealed class GoogleBooksApiClient : ApiClientBase<GoogleBooksApiClient>, IGoogleBooksApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly GoogleBooksApiOptions _options;

        public GoogleBooksApiClient(
            HttpClient httpClient,
            IOptions<GoogleBooksApiOptions> options,
            ErrorEventLogger<GoogleBooksApiClient> errorEventLogger)
            : base(errorEventLogger, "GoogleBooks")
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<Result<GoogleBooksVolumeResponse>> GetBookByIdAsync(
            string volumeId,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(GetBookByIdAsync), OperationType.Get, fieldName: volumeId);

            return await SendAndMapAsync<GoogleBooksVolumeResponse>(
                ct => _httpClient.GetAsync(BuildRequestUri($"volumes/{volumeId}"), ct),
                errorContext,
                cancellationToken);
        }

        public async Task<Result<GoogleBooksSearchResponse>> SearchBooksAsync(
            List<string> queryParameters,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchBooksAsync), OperationType.GetCollection);

            var requestUri = BuildRequestUri($"volumes?{string.Join("&", queryParameters)}");

            return await SendAndMapAsync<GoogleBooksSearchResponse>(
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
                entityName: entityName ?? "Google Books Volume",
                fieldName: fieldName);
        }
    }
}
