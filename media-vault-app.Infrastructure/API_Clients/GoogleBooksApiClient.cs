using System.ComponentModel.DataAnnotations;
using media_vault_app.Application.DTOs.GoogleBooks;
using media_vault_app.Application.Interfaces.Clients;
using Microsoft.Extensions.Options;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.API_Clients
{

    public sealed class GoogleBooksApiOptions
    {
        public const string SectionName = "ExternalApis:GoogleBooksApi";

        [Required]
        public string BaseUrl { get; set; } = string.Empty;

        [Required]
        public string ApiKey { get; set; } = string.Empty;
    }

    public sealed class GoogleBooksApiClient : IGoogleBooksApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly GoogleBooksApiOptions _options;

        public GoogleBooksApiClient(HttpClient httpClient, IOptions<GoogleBooksApiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<Result<GoogleBooksVolumeResponse>> GetBookAsync(string volumeId, CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(GetBookAsync), OperationType.Get, fieldName: volumeId);

            using var response = await _httpClient.GetAsync(BuildRequestUri($"volumes/{volumeId}"), cancellationToken);

            return await response.MapAsync<GoogleBooksVolumeResponse>(errorContext, cancellationToken);
        }

        public async Task<Result<GoogleBooksSearchResponse>> SearchBooksAsync(
            List<string> queryParameters,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchBooksAsync), OperationType.GetCollection);

            var requestUri = BuildRequestUri($"volumes?{string.Join("&", queryParameters)}");

            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            return await response.MapAsync<GoogleBooksSearchResponse>(errorContext, cancellationToken);
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
                entityName: entityName ?? "Google Books Volume",
                fieldName: fieldName);
        }
    }
}
