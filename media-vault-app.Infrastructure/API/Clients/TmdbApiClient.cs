using System.ComponentModel.DataAnnotations;
using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Movie;
using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Shared;
using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.TvSeries;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Domain.Enums;
using Microsoft.Extensions.Options;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Validation;

namespace media_vault_app.Infrastructure.API.Clients
{
    public sealed class TmdbApiOptions
    {

        public const string SectionName = "ExternalApis:Tmdb";

        [Required]
        public string BaseUrl { get; set; } = string.Empty;

        [Required]
        public string ApiAccessToken { get; set; } = string.Empty;
    }

    public class TmdbApiClient : ApiClientBase, ITmdbApiClient
    {

        private readonly HttpClient _httpClient;
        private readonly TmdbApiOptions _options;

        public TmdbApiClient(
            HttpClient httpClient,
            IOptions<TmdbApiOptions> options,
            IErrorLogger errorLogger,
            IErrorLogPolicy errorLogPolicy)
            : base(errorLogger, errorLogPolicy)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<Result<TmdbTvSeriesDetailedResult>> GetTvSeriesByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(GetTvSeriesByIdAsync), OperationType.Get);

            return await SendAndMapAsync<TmdbTvSeriesDetailedResult>(
                ct => _httpClient.GetAsync(BuildRequestUri($"tv/{id}"), ct),
                errorContext,
                cancellationToken);
        }

        public async Task<Result<TmdbMovieDetailedResponse>> GetMovieByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(GetMovieByIdAsync), OperationType.Get);

            return await SendAndMapAsync<TmdbMovieDetailedResponse>(
                ct => _httpClient.GetAsync(BuildRequestUri($"movie/{id}"), ct),
                errorContext,
                cancellationToken);
        }

        public async Task<Result<TmdbSearchResponse>> SearchAsync(
            List<string> queryParameters,
            MediaType mediaType,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchAsync), OperationType.GetCollection);

            string? endpoint = mediaType switch
            {
                MediaType.Movie => $"movie?{string.Join("&", queryParameters)}",
                MediaType.TvSeries => $"tv?{string.Join("&", queryParameters)}",
                _ => null
            };

            if (endpoint is null)
            {
                var invalidMediaTypeError = MediaVaultValidationError.InvalidFormat(
                    errorContext with { FieldName = nameof(mediaType) },
                    $"Unsupported media type: {mediaType}");

                return Result<TmdbSearchResponse>.ValidationFailure([invalidMediaTypeError], invalidMediaTypeError.UserMessage);
            }

            var requestUri = BuildRequestUri($"search/{endpoint}");

            return await SendAndMapAsync<TmdbSearchResponse>(
                ct => _httpClient.GetAsync(requestUri, ct),
                errorContext,
                cancellationToken);
        }

        private static string BuildRequestUri(string pathAndQuery)
        {
            return pathAndQuery;
        }


        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? entityName = null, string? fieldName = null)
        {
            return new ErrorContext(
                operation: operation,
                entityName: entityName ?? "Tmdb",
                fieldName: fieldName);
        }

    }
}
