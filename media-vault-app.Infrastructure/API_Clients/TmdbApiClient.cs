using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.Tmdb.Movie;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.API_Clients
{
    public class TmdbApiClient : ITmdbApiClient
    {

        private readonly HttpClient _httpClient;

        public TmdbApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Result<TmdbResult>> GetByIdAsync(int id, MediaEntryType mediaType, CancellationToken cancellationToken = default)
        {

            string? endpoint = mediaType switch
            {
                MediaEntryType.MovieEntry => $"movie/{id}",
                MediaEntryType.SeriesEntry => $"tv/{id}",
                _ => null
            };

            if (endpoint is null)
            {
                var mediaTypeErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get, fieldName: $"{nameof(mediaType)}");

                mediaTypeErrorContext.DescriptionSuffix = "Failed to determine API endpoint for media type.";

                var invalidMediaTypeError = ValidationError.InvalidFormat(mediaTypeErrorContext, $"Unsupported media type: {mediaType}");

                return Result<TmdbResult>.ValidationFailure([invalidMediaTypeError], mediaTypeErrorContext.DescriptionSuffix);
            }

            using var response = await _httpClient.GetAsync(BuildRequestUri($"{endpoint}"), cancellationToken);

            var httpResponseErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get, fieldName: $"{id}");

            return await response.MapAsync<TmdbResult>(httpResponseErrorContext, cancellationToken);
        }


        public async Task<Result<TmdbSearchResponse>> SearchAsync(
            List<string> queryParameters,
            MediaEntryType mediaType,
            CancellationToken cancellationToken = default)
        {

            string? endpoint = mediaType switch
            {
                MediaEntryType.MovieEntry => $"movie?{string.Join("&", queryParameters)}",
                MediaEntryType.SeriesEntry => $"tv?{string.Join("&", queryParameters)}",
                _ => null
            };

            if (endpoint is null)
            {
                var mediaTypeErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get, fieldName: $"{nameof(mediaType)}");

                mediaTypeErrorContext.DescriptionSuffix = "Failed to determine API endpoint for media type.";

                var invalidMediaTypeError = ValidationError.InvalidFormat(mediaTypeErrorContext, $"Unsupported media type: {mediaType}");

                return Result<TmdbSearchResponse>.ValidationFailure([invalidMediaTypeError], mediaTypeErrorContext.DescriptionSuffix);
            }

            var requestUri = BuildRequestUri($"search/{endpoint}");

            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            var httpResponseErrorContext = DefineErrorContext(nameof(SearchAsync), OperationType.GetCollection);

            return await response.MapAsync<TmdbSearchResponse>(httpResponseErrorContext, cancellationToken);
        }

        private static string BuildRequestUri(string pathAndQuery)
        {
            return pathAndQuery;
        }


        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? entityName = null, string? fieldName = null)
        {
            return new ErrorContext(
                layer: "Infrastructure",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: entityName ?? "Tmdb",
                fieldName: fieldName);
        }

    }
}
