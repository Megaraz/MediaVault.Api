using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.Tmdb.TVSeries;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Interfaces.Services;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services.API
{
    public class TmdbTvSeriesApiService : ITmdbTvSeriesApiService
    {
        private readonly ITmdbApiClient _client;

        public TmdbTvSeriesApiService(ITmdbApiClient client)
        {
            _client = client;
        }


        public async Task<Result<TvSearchResultDto>> GetTvSeriesByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var idValidationErrorContext = DefineErrorContext(nameof(GetTvSeriesByIdAsync), OperationType.Get);

            if (!id.IsValidId(idValidationErrorContext, out var idError))
            {
                idValidationErrorContext.DescriptionSuffix = $"Invalid TV series ID: {id}.";
                return Result<TvSearchResultDto>.ValidationFailure([idError], idValidationErrorContext.DescriptionSuffix);
            }

            var result = await _client.GetTvSeriesAsync(id, cancellationToken);
            return result.Map(MapToTvSearchResult);

        }
        public async Task<Result<IReadOnlyList<TvSearchResultDto>>> SearchTvSeriesAsync(
            string search,
            int page = 1,
            int pageSize = 10,
            string? ordering = null,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchTvSeriesAsync), OperationType.GetCollection);
            List<ValidationError> errors = new();

            errorContext.FieldName = nameof(search);
            if (search.IsNullOrWhiteSpace(errorContext, out var searchError))
            {
                errors.Add(searchError);
            }

            if (page < 1)
            {
                errorContext.DescriptionSuffix = "Page number must be greater than 0.";
                errorContext.FieldName = nameof(page);

                var pageError = ValidationError.OutOfRange(errorContext, "Greater than 0");
                errors.Add(pageError);
            }

            if (pageSize < 1)
            {
                errorContext.DescriptionSuffix = "Page size must be greater than 0.";
                errorContext.FieldName = nameof(pageSize);

                var pageSizeError = ValidationError.OutOfRange(errorContext, "Greater than 0");
                errors.Add(pageSizeError);
            }

            if (errors.Any())
            {
                return Result<IReadOnlyList<TvSearchResultDto>>.ValidationFailure(errors, "TMDB TV series search validation failed.");
            }

            var queryParameters = new List<string>
            {
                $"search={Uri.EscapeDataString(search)}",
                $"page={page}"
            };


            var result = await _client.SearchTvSeriesAsync(queryParameters, cancellationToken);

            //if (!string.IsNullOrWhiteSpace(ordering))
            //{
            //    queryParameters.Add($"ordering={Uri.EscapeDataString(ordering)}");
            //}


            return result.Map(searchResponse => MapToTvSearchResult(searchResponse.Results));

        }



        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? entityName = null, string? fieldName = null)
        {
            return new ErrorContext(
                layer: "Application",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: entityName ?? "Tmdb",
                fieldName: fieldName);
        }

        private static IReadOnlyList<TvSearchResultDto> MapToTvSearchResult(IReadOnlyList<TmdbTvResult>? tmdbTvSeries)
        {
            return tmdbTvSeries?.Select(MapToTvSearchResult).ToArray() ?? Array.Empty<TvSearchResultDto>();
        }

        private static TvSearchResultDto MapToTvSearchResult(TmdbTvResult tmdbTvSeries)
        {
            return new TvSearchResultDto(
                tmdbTvSeries.Id,
                tmdbTvSeries.Name ?? string.Empty,
                tmdbTvSeries.PosterPath);
        }
    }
}
