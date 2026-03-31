using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.Tmdb.Movie;
using media_vault_app.Application.DTOs.Tmdb.TVSeries;
using media_vault_app.Application.Interfaces.Clients;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.API_Clients
{
    public sealed record TmdbApiOptions(string BaseUrl, string ApiKey);


    public class TmdbApiClient : ITmdbApiClient
    {

        private readonly HttpClient _httpClient;

        public TmdbApiClient(HttpClient httpClient, TmdbApiOptions options)
        {
            _httpClient = httpClient;
        }

        public async Task<Result<TmdbMovieResult>> GetMovieAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(BuildRequestUri($"movie/{id}"), cancellationToken);

            var httpResponseErrorContext = DefineErrorContext(nameof(GetMovieAsync), OperationType.Get, fieldName: $"{id}");

            return await response.MapAsync<TmdbMovieResult>(httpResponseErrorContext, cancellationToken);
        }
        public async Task<Result<TmdbTvResult>> GetTvSeriesAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(BuildRequestUri($"tv/{id}"), cancellationToken);

            var httpResponseErrorContext = DefineErrorContext(nameof(GetTvSeriesAsync), OperationType.Get, fieldName: $"{id}");

            return await response.MapAsync<TmdbTvResult>(httpResponseErrorContext, cancellationToken);
        }
        public async Task<Result<TmdbMovieSearchResponse>> SearchMoviesAsync(
            List<string> queryParameters,
            CancellationToken cancellationToken = default)
        {
            var requestUri = BuildRequestUri($"search/movie?{string.Join("&", queryParameters)}");

            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            var errorContext = DefineErrorContext(nameof(SearchMoviesAsync), OperationType.GetCollection);

            return await response.MapAsync<TmdbMovieSearchResponse>(errorContext, cancellationToken);
        }

        public async Task<Result<TmdbTvSearchResponse>> SearchTvSeriesAsync(
            List<string> queryParameters,
            CancellationToken cancellationToken = default)
        {
            var requestUri = BuildRequestUri($"search/tv?{string.Join("&", queryParameters)}");
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            var errorContext = DefineErrorContext(nameof(SearchTvSeriesAsync), OperationType.GetCollection);

            return await response.MapAsync<TmdbTvSearchResponse>(errorContext, cancellationToken);
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
