using System.ComponentModel.DataAnnotations;
using media_vault_app.Application.DTOs.External_API_Contracts.Rawg;
using media_vault_app.Application.Interfaces.Clients;
using Microsoft.Extensions.Options;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using Rasmus.SharedKernel.ResultPatternCompatibility;

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


    public sealed class RawgApiClient : ApiClientBase, IRawgApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly RawgApiOptions _options;

        public RawgApiClient(
            HttpClient httpClient,
            IOptions<RawgApiOptions> options,
            IErrorLogger errorLogger,
            IErrorLogPolicy errorLogPolicy)
            : base(errorLogger, errorLogPolicy)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<Result<RawgGameDetailedResponse>> GetGameByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(GetGameByIdAsync), OperationType.Get);

            try
            {
                using var response = await _httpClient.GetAsync(BuildRequestUri($"games/{id}"), cancellationToken);

                var result = await response.MapToResultAsync<RawgGameDetailedResponse>(errorContext, cancellationToken);

                await LogIfNeededAsync(result.PrimaryError, cancellationToken);

                return result;
            }
            catch (OperationCanceledException)
            {
                return Result<RawgGameDetailedResponse>.Failure(MediaVaultErrors.Cancelled(errorContext));
            }
            catch (HttpRequestException exception)
            {
                var error = HttpError.TransportFailure(errorContext, exception);
                await LogIfNeededAsync(error, CancellationToken.None);
                return Result<RawgGameDetailedResponse>.Failure(error);
            }
        }

        public async Task<Result<RawgSearchResponse>> SearchGamesAsync(
            List<string> queryParameters,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchGamesAsync), OperationType.GetCollection);

            try
            {
                var requestUri = BuildRequestUri($"games?{string.Join("&", queryParameters)}");

                using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

                var result = await response.MapToResultAsync<RawgSearchResponse>(errorContext, cancellationToken);

                await LogIfNeededAsync(result.PrimaryError, cancellationToken);

                return result;
            }
            catch (OperationCanceledException)
            {
                return Result<RawgSearchResponse>.Failure(MediaVaultErrors.Cancelled(errorContext));
            }
            catch (HttpRequestException exception)
            {
                var error = HttpError.TransportFailure(errorContext, exception);
                await LogIfNeededAsync(error, CancellationToken.None);
                return Result<RawgSearchResponse>.Failure(error);
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
                entityName: entityName ?? "Rawg Game",
                fieldName: fieldName);
        }

    }


}
