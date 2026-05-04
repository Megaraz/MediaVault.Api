using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Movie;
using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Shared;
using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.TvSeries;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Domain.Enums;
using Microsoft.Extensions.Options;
using Rasmus.SharedKernel.ResultPattern;

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

    public class TmdbApiClient : ITmdbApiClient
    {

        private readonly HttpClient _httpClient;
        private readonly TmdbApiOptions _options;

        public TmdbApiClient(HttpClient httpClient, IOptions<TmdbApiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<Result<TmdbTvSeriesDetailedResult>> GetTvSeriesByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetTvSeriesByIdAsync), OperationType.Get);

            if (!id.IsValidId(baseErrorContext, out var idValidationError))
            {
                return Result<TmdbTvSeriesDetailedResult>.ValidationFailure([idValidationError]);
            }

            using var response = await _httpClient.GetAsync(BuildRequestUri($"tv/{id}"), cancellationToken);

            var httpResponseErrorContext = baseErrorContext with 
            { 
                FieldName = $"{id}"
            };

            return await response.MapToResultAsync<TmdbTvSeriesDetailedResult>(httpResponseErrorContext, cancellationToken);
        }
        public async Task<Result<TmdbMovieDetailedResponse>> GetMovieByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetMovieByIdAsync), OperationType.Get);

            if (!id.IsValidId(baseErrorContext, out var idValidationError))
            {
                return Result<TmdbMovieDetailedResponse>.ValidationFailure([idValidationError]);
            }

            using var response = await _httpClient.GetAsync(BuildRequestUri($"movie/{id}"), cancellationToken);

            var httpResponseErrorContext = baseErrorContext with 
            { 
                FieldName = $"{id}"
            };

            return await response.MapToResultAsync<TmdbMovieDetailedResponse>(httpResponseErrorContext, cancellationToken);
        }
        //public async Task<Result<TmdbSearchResult>> GetByIdAsync(int id, MediaType mediaType, CancellationToken cancellationToken = default)
        //{
        //    var baseErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

        //    if (!id.IsValidId(baseErrorContext, out var idValidationError))
        //    {
        //        return Result<TmdbSearchResult>.ValidationFailure([idValidationError]);
        //    }

        //    string? endpoint = mediaType switch
        //    {
        //        MediaType.Movie => $"movie/{id}",
        //        MediaType.TvSeries => $"tv/{id}",
        //        _ => null
        //    };


        //    if (endpoint is null)
        //    {
        //        var mediaTypeErrorContext = baseErrorContext with 
        //        { 
        //            FieldName = $"{nameof(mediaType)}",
        //            DescriptionSuffix = "Failed to determine API endpoint for media type."
        //        };

        //        var invalidMediaTypeError = ValidationError.InvalidFormat(mediaTypeErrorContext, $"Unsupported media type: {mediaType}");

        //        return Result<TmdbSearchResult>.ValidationFailure([invalidMediaTypeError], mediaTypeErrorContext.DescriptionSuffix);
        //    }

        //    using var response = await _httpClient.GetAsync(BuildRequestUri($"{endpoint}"), cancellationToken);

        //    var httpResponseErrorContext = baseErrorContext with 
        //    { 
        //        FieldName = $"{id}"
        //    };

        //    return await response.MapToResultAsync<TmdbSearchResult>(httpResponseErrorContext, cancellationToken);
        //}


        public async Task<Result<TmdbSearchResponse>> SearchAsync(
            List<string> queryParameters,
            MediaType mediaType,
            CancellationToken cancellationToken = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(SearchAsync), OperationType.GetCollection);

            string? endpoint = mediaType switch
            {
                MediaType.Movie => $"movie?{string.Join("&", queryParameters)}",
                MediaType.TvSeries => $"tv?{string.Join("&", queryParameters)}",
                _ => null
            };

            if (endpoint is null)
            {
                var mediaTypeErrorContext = baseErrorContext with 
                { 
                    FieldName = $"{nameof(mediaType)}",
                    DescriptionSuffix = "Failed to determine API endpoint for media type."
                };

                var invalidMediaTypeError = ValidationError.InvalidFormat(mediaTypeErrorContext, $"Unsupported media type: {mediaType}");

                return Result<TmdbSearchResponse>.ValidationFailure([invalidMediaTypeError], mediaTypeErrorContext.DescriptionSuffix);
            }

            var requestUri = BuildRequestUri($"search/{endpoint}");

            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            return await response.MapToResultAsync<TmdbSearchResponse>(baseErrorContext, cancellationToken);
        }

        private static string BuildRequestUri(string pathAndQuery)
        {
            return pathAndQuery;
        }


        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? entityName = null, string? fieldName = null)
        {
            return new ErrorContext(
                Layer: "Infrastructure",
                ServiceName: GetType().Name,
                MethodName: methodName,
                Operation: operation,
                EntityName: entityName ?? "Tmdb",
                FieldName: fieldName);
        }

    }
}
