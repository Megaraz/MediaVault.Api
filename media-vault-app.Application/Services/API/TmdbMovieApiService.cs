using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.Tmdb.Movie;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Interfaces.Services;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services.API
{
    public class TmdbMovieApiService : ITmdbMovieApiService
    {
        private readonly ITmdbApiClient _client;

        public TmdbMovieApiService(ITmdbApiClient client)
        {
            _client = client;
        }


        public async Task<Result<MovieSearchResultDto>> GetMovieByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var idValidationErrorContext = DefineErrorContext(nameof(GetMovieByIdAsync), OperationType.Get);

            if (!id.IsValidId(idValidationErrorContext, out var idError))
            {
                idValidationErrorContext.DescriptionSuffix = $"Invalid movie ID: {id}.";
                return Result<MovieSearchResultDto>.ValidationFailure([idError], idValidationErrorContext.DescriptionSuffix);
            }

            var result = await _client.GetMovieAsync(id, cancellationToken);
            return result.Map(MapToMovieSearchResult);

        }
        public async Task<Result<IReadOnlyList<MovieSearchResultDto>>> SearchMoviesAsync(
            string search,
            int page = 1,
            int pageSize = 10,
            string? ordering = null,
            CancellationToken cancellationToken = default)
        {
            var errorContext = DefineErrorContext(nameof(SearchMoviesAsync), OperationType.GetCollection);
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
                return Result<IReadOnlyList<MovieSearchResultDto>>.ValidationFailure(errors, "TMDB movie search validation failed.");
            }

            var queryParameters = new List<string>
            {
                $"query={Uri.EscapeDataString(search)}",
                $"page={page}"
            };

            var result = await _client.SearchMoviesAsync(queryParameters, cancellationToken);

            //if (!string.IsNullOrWhiteSpace(ordering))
            //{
            //    queryParameters.Add($"ordering={Uri.EscapeDataString(ordering)}");
            //}

            return result.Map(searchResponse => MapToMovieSearchResult(searchResponse.Results));

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

        private static IReadOnlyList<MovieSearchResultDto> MapToMovieSearchResult(IReadOnlyList<TmdbMovieResult>? tmdbMovies)
        {
            return tmdbMovies?.Select(MapToMovieSearchResult).ToArray() ?? Array.Empty<MovieSearchResultDto>();
        }

        private static MovieSearchResultDto MapToMovieSearchResult(TmdbMovieResult tmdbMovie)
        {
            return new MovieSearchResultDto(
                tmdbMovie.Id,
                tmdbMovie.Title ?? string.Empty,
                BuildImageUrl(tmdbMovie.PosterPath));
        }

        private static string? BuildImageUrl(string? path)
        {
            const string tmdbImageBaseUrl = "https://image.tmdb.org/t/p/w500";
            return string.IsNullOrEmpty(path) ? null : $"{tmdbImageBaseUrl}{path}";
        }
    }
}
