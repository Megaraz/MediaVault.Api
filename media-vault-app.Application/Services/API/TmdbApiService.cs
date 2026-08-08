using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Movie;
using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Shared;
using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.TvSeries;
using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.DTOs.Tmdb;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Domain.Enums;
using Microsoft.Extensions.Logging;
using Rasmus.SharedKernel.Pagination;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Results;
using Rasmus.SharedKernel.Validation;

namespace media_vault_app.Application.Services.API
{
    public class TmdbApiService : ITmdbApiService
    {
        private readonly ITmdbApiClient _client;
        private readonly ILogger<TmdbApiService> _logger;

        public TmdbApiService(ITmdbApiClient client, ILogger<TmdbApiService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<Result<TmdbTvSeriesDetailedDto>> GetTvSeriesByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var idValidationErrorContext = DefineErrorContext(nameof(GetTvSeriesByIdAsync), OperationType.Get);

            if (id.IsNotValidMediaVaultId(idValidationErrorContext, out var idError))
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, [idError], nameof(TmdbApiService), nameof(GetTvSeriesByIdAsync), idValidationErrorContext);
                return Result<TmdbTvSeriesDetailedDto>.ValidationFailure([idError], MediaVaultResultMessages.ValidationFailure);
            }

            var clientResult = await _client.GetTvSeriesByIdAsync(id, cancellationToken);

            return clientResult.Map(ToDetailedDto);
        }
        public async Task<Result<TmdbMovieDetailedDto>> GetMovieByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var idValidationErrorContext = DefineErrorContext(nameof(GetMovieByIdAsync), OperationType.Get);

            if (id.IsNotValidMediaVaultId(idValidationErrorContext, out var idError))
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, [idError], nameof(TmdbApiService), nameof(GetMovieByIdAsync), idValidationErrorContext);
                return Result<TmdbMovieDetailedDto>.ValidationFailure([idError], MediaVaultResultMessages.ValidationFailure);
            }

            var clientResult = await _client.GetMovieByIdAsync(id, cancellationToken);

            return clientResult.Map(ToDetailedDto);
        }

        public async Task<Result<IReadOnlyList<MediaEntryExternalSearchResultDto>>> SearchAsync(
            string search,
            MediaType mediaType,
            int page = 1,
            int pageSize = 10,
            string? ordering = null,
            CancellationToken cancellationToken = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(SearchAsync), OperationType.GetCollection);

            List<ValidationError> errors = new();

            if (search.IsMissingMediaVaultValue(baseErrorContext with { FieldName = nameof(search) }, out var searchError))
            {
                errors.Add(searchError);
            }

            var pagination = PaginationParameters.Normalize(page, pageSize);
            page = pagination.PageNumber;
            pageSize = pagination.PageSize;

            if (errors.Any())
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, errors, nameof(TmdbApiService), nameof(SearchAsync), baseErrorContext);
                return Result<IReadOnlyList<MediaEntryExternalSearchResultDto>>.ValidationFailure(errors, "TMDB search validation failed.");
            }

            var queryParameters = new List<string>
            {
                $"query={Uri.EscapeDataString(search)}",
                $"page={page}"
            };

            var clientResult = await _client.SearchAsync(queryParameters, mediaType, cancellationToken);

            return clientResult.Map(searchResponse => MapToSearchResults(searchResponse.Results, mediaType));
        }

        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? entityName = null, string? fieldName = null)
        {
            return new ErrorContext(
                operation: operation,
                entityName: entityName ?? "Tmdb",
                fieldName: fieldName);
        }

        private static IReadOnlyList<MediaEntryExternalSearchResultDto> MapToSearchResults(IReadOnlyList<TmdbSearchResult>? results, MediaType mediaType)
        {
            return results?.Select(r => MapToSearchResult(r, mediaType)).ToArray() ?? Array.Empty<MediaEntryExternalSearchResultDto>();
        }

        private static TmdbTvSeriesDetailedDto ToDetailedDto(TmdbTvSeriesDetailedResult tvSeriesResult)
        {
            return new TmdbTvSeriesDetailedDto
            (
                TmdbBackdropPath: BuildImageUrl(tvSeriesResult.BackdropPath),
                TmdbFirstAirDate: tvSeriesResult.FirstAirDate ?? string.Empty,
                TmdbGenres: tvSeriesResult.Genres?.Select(g =>
                    new TmdbGenreDto
                    (
                        TmdbGenreId: g.Id,
                        TmdbGenreName: g.Name
                    )).ToArray(),
                TmdbTvSeriesId: tvSeriesResult.Id,
                TmdbLastAirDate: tvSeriesResult.LastAirDate ?? string.Empty,
                TmdbName: tvSeriesResult.Name ?? string.Empty,
                TmdbNumberOfEpisodes: tvSeriesResult.NumberOfEpisodes,
                TmdbNumberOfSeasons: tvSeriesResult.NumberOfSeasons,
                TmdbOverview: tvSeriesResult.Overview ?? string.Empty,
                TmdbPosterPath: BuildImageUrl(tvSeriesResult.PosterPath),
                TmdbSeasons: tvSeriesResult.Seasons?.Select(s =>
                    new TmdbSeasonDto
                    (
                        TmdbAirDate: s.AirDate ?? string.Empty,
                        TmdbEpisodeCount: s.EpisodeCount,
                        TmdbName: s.Name ?? string.Empty,
                        TmdbOverview: s.Overview ?? string.Empty,
                        TmdbPosterPath: BuildImageUrl(s.PosterPath),
                        TmdbSeasonNumber: s.SeasonNumber
                    )).ToArray(),
                TmdbStatus: tvSeriesResult.Status ?? string.Empty
            );
        }

        private static TmdbMovieDetailedDto ToDetailedDto(TmdbMovieDetailedResponse movieResult)
        {
            return new TmdbMovieDetailedDto
            (
                TmdbBackdropPath: BuildImageUrl(movieResult.PosterPath),
                TmdbReleaseDate: movieResult.ReleaseDate ?? string.Empty,
                TmdbGenres: movieResult.Genres.Select(g =>
                    new TmdbGenreDto
                    (
                        TmdbGenreId: g.Id,
                        TmdbGenreName: g.Name
                    )).ToArray(),
                TmdbMovieId: movieResult.Id,
                TmdbOverview: movieResult.Overview ?? string.Empty,
                TmdbPosterPath: BuildImageUrl(movieResult.PosterPath),
                TmdbTitle: movieResult.Title ?? string.Empty,
                TmdbRunTimeMinutes: movieResult.RunTime
            );
        }

        private static MediaEntryExternalSearchResultDto MapToSearchResult(TmdbSearchResult result, MediaType mediaType)
        {
            return new MediaEntryExternalSearchResultDto(
                result.Id.ToString(),
                result.Title ?? result.Name ?? string.Empty,
                BuildImageUrl(result.PosterPath),
                mediaType);
        }

        private static string? BuildImageUrl(string? path)
        {
            const string tmdbImageBaseUrl = "https://image.tmdb.org/t/p/w500";
            return string.IsNullOrEmpty(path) ? null : $"{tmdbImageBaseUrl}{path}";
        }
    }
}
