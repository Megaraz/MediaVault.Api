using media_vault_app.Application.DTOs.GoogleBooks;
using media_vault_app.Application.Interfaces.Clients;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.API_Clients
{
    public sealed class GoogleBooksApiClient : IGoogleBooksApiClient
    {
        private readonly HttpClient _httpClient;

        public GoogleBooksApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Result<GoogleBooksVolumeResponse>> GetBookAsync(string volumeId, CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(GetBookAsync), OperationType.Get, fieldName: volumeId);

            using var response = await _httpClient.GetAsync($"volumes/{volumeId}", cancellationToken);

            return await response.MapAsync<GoogleBooksVolumeResponse>(errorContext, cancellationToken);
        }

        public async Task<Result<GoogleBooksSearchResponse>> SearchBooksAsync(
            List<string> queryParameters,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchBooksAsync), OperationType.GetCollection);

            var requestUri = $"volumes?{string.Join("&", queryParameters)}";

            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            return await response.MapAsync<GoogleBooksSearchResponse>(errorContext, cancellationToken);
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
