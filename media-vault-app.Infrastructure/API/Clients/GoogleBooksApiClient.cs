using System.ComponentModel.DataAnnotations;
using media_vault_app.Application.DTOs.External_API_Contracts.GoogleBooks;
using media_vault_app.Application.Interfaces.Clients;
using Microsoft.Extensions.Options;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using Rasmus.SharedKernel.ResultPatternCompatibility;

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

    public sealed class GoogleBooksApiClient : ApiClientBase, IGoogleBooksApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly GoogleBooksApiOptions _options;

        public GoogleBooksApiClient(
            HttpClient httpClient,
            IOptions<GoogleBooksApiOptions> options,
            IErrorLogger errorLogger,
            IErrorLogPolicy errorLogPolicy)
            : base(errorLogger, errorLogPolicy)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<Result<GoogleBooksVolumeResponse>> GetBookByIdAsync(
            string volumeId,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(GetBookByIdAsync), OperationType.Get, fieldName: volumeId);

            try
            {
                using var response = await _httpClient.GetAsync(
                    BuildRequestUri($"volumes/{volumeId}"),
                    cancellationToken);

                var result = await response.MapToResultAsync<GoogleBooksVolumeResponse>(
                    errorContext,
                    cancellationToken);

                await LogIfNeededAsync(result.PrimaryError, cancellationToken);

                return result;
            }
            catch (OperationCanceledException)
            {
                return Result<GoogleBooksVolumeResponse>.Failure(MediaVaultErrors.Cancelled(errorContext));
            }
            catch (HttpRequestException exception)
            {
                var error = HttpError.TransportFailure(errorContext, exception);
                await LogIfNeededAsync(error, CancellationToken.None);
                return Result<GoogleBooksVolumeResponse>.Failure(error);
            }
        }

        public async Task<Result<GoogleBooksSearchResponse>> SearchBooksAsync(
            List<string> queryParameters,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchBooksAsync), OperationType.GetCollection);

            try
            {
                var requestUri = BuildRequestUri($"volumes?{string.Join("&", queryParameters)}");

                using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

                var result = await response.MapToResultAsync<GoogleBooksSearchResponse>(
                    errorContext,
                    cancellationToken);

                await LogIfNeededAsync(result.PrimaryError, cancellationToken);

                return result;
            }
            catch (OperationCanceledException)
            {
                return Result<GoogleBooksSearchResponse>.Failure(MediaVaultErrors.Cancelled(errorContext));
            }
            catch (HttpRequestException exception)
            {
                var error = HttpError.TransportFailure(errorContext, exception);
                await LogIfNeededAsync(error, CancellationToken.None);
                return Result<GoogleBooksSearchResponse>.Failure(error);
            }
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
