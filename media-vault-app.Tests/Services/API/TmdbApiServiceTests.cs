using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Movie;
using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.Shared;
using media_vault_app.Application.DTOs.External_API_Contracts.Tmdb.TvSeries;
using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Services.API;
using media_vault_app.Domain.Enums;
using media_vault_app.Tests.TestHelpers;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;

namespace media_vault_app.Tests.Services.API
{
    public class TmdbApiServiceTests
    {
        [Fact]
        public async Task GetMovieByIdAsync_Should_ReturnValidationFailure_When_IdIsInvalid()
        {
            var client = new FakeTmdbApiClient();
            var service = new TmdbApiService(client, ServiceTestLogger.Create<TmdbApiService>());

            var result = await service.GetMovieByIdAsync(0);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Single(result.ValidationErrors);
            Assert.Equal(0, client.GetMovieByIdCallCount);
        }

        [Fact]
        public async Task GetMovieByIdAsync_Should_Map_Detailed_Response()
        {
            var client = new FakeTmdbApiClient(
                getMovieByIdResult: Result<TmdbMovieDetailedResponse>.Success(new TmdbMovieDetailedResponse
                {
                    Id = 77,
                    BackdropPath = "/backdrop.jpg",
                    PosterPath = "/poster.jpg",
                    ReleaseDate = "2025-03-01",
                    Genres = [new TmdbGenre { Id = 1, Name = "Action" }],
                    Overview = "Movie overview",
                    Title = "The Movie",
                    RunTime = 143
                }));

            var service = new TmdbApiService(client, ServiceTestLogger.Create<TmdbApiService>());

            var result = await service.GetMovieByIdAsync(77);

            Assert.True(result.IsSuccess);
            Assert.Equal(77, result.Value.TmdbMovieId);
            Assert.Equal("The Movie", result.Value.TmdbTitle);
            Assert.Equal("Movie overview", result.Value.TmdbOverview);
            Assert.Equal("https://image.tmdb.org/t/p/w500/poster.jpg", result.Value.TmdbPosterPath);
            Assert.Equal("https://image.tmdb.org/t/p/w500/backdrop.jpg", result.Value.TmdbBackdropPath);
            var genre = Assert.Single(result.Value.TmdbGenres);
            Assert.Equal(1, genre.TmdbGenreId);
            Assert.Equal("Action", genre.TmdbGenreName);
        }

        [Fact]
        public async Task GetMovieByIdAsync_Should_Map_Missing_Backdrop_As_Null()
        {
            var client = new FakeTmdbApiClient(
                getMovieByIdResult: Result<TmdbMovieDetailedResponse>.Success(new TmdbMovieDetailedResponse
                {
                    Id = 77,
                    PosterPath = "/poster.jpg"
                }));

            var service = new TmdbApiService(client, ServiceTestLogger.Create<TmdbApiService>());

            var result = await service.GetMovieByIdAsync(77);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.TmdbBackdropPath);
            Assert.Equal("https://image.tmdb.org/t/p/w500/poster.jpg", result.Value.TmdbPosterPath);
        }

        [Fact]
        public async Task GetTvSeriesByIdAsync_Should_Map_Detailed_Response()
        {
            var client = new FakeTmdbApiClient(
                getTvSeriesByIdResult: Result<TmdbTvSeriesDetailedResult>.Success(new TmdbTvSeriesDetailedResult
                {
                    Id = 15,
                    BackdropPath = "/backdrop.jpg",
                    FirstAirDate = "2024-01-01",
                    Genres = [new TmdbGenre { Id = 2, Name = "Drama" }],
                    LastAirDate = "2024-03-01",
                    Name = "The Series",
                    NumberOfEpisodes = 8,
                    NumberOfSeasons = 1,
                    Overview = "Series overview",
                    PosterPath = "/poster.jpg",
                    Seasons = [new TmdbSeason { AirDate = "2024-01-01", EpisodeCount = 8, Name = "Season 1", Overview = "Season overview", PosterPath = "/season.jpg", SeasonNumber = 1 }],
                    Status = "Ended"
                }));

            var service = new TmdbApiService(client, ServiceTestLogger.Create<TmdbApiService>());

            var result = await service.GetTvSeriesByIdAsync(15);

            Assert.True(result.IsSuccess);
            Assert.Equal(15, result.Value.TmdbTvSeriesId);
            Assert.Equal("The Series", result.Value.TmdbName);
            Assert.Equal("https://image.tmdb.org/t/p/w500/backdrop.jpg", result.Value.TmdbBackdropPath);
            Assert.Equal("https://image.tmdb.org/t/p/w500/poster.jpg", result.Value.TmdbPosterPath);
            var season = Assert.Single(result.Value.TmdbSeasons!);
            Assert.Equal("https://image.tmdb.org/t/p/w500/season.jpg", season.TmdbPosterPath);
        }

        [Fact]
        public async Task GetTvSeriesByIdAsync_Should_Propagate_ClientFailure()
        {
            var expectedError = MediaVaultErrors.NotFound(new ErrorContext(
                operation: OperationType.Get,
                entityName: "Tmdb"));

            var client = new FakeTmdbApiClient(
                getTvSeriesByIdResult: Result<TmdbTvSeriesDetailedResult>.Failure(expectedError, "Series not found."));

            var service = new TmdbApiService(client, ServiceTestLogger.Create<TmdbApiService>());

            var result = await service.GetTvSeriesByIdAsync(99);

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
            Assert.Equal("Series not found.", result.Message);
        }

        [Fact]
        public async Task SearchAsync_Should_ReturnValidationFailure_When_SearchIsBlank()
        {
            var client = new FakeTmdbApiClient();
            var service = new TmdbApiService(client, ServiceTestLogger.Create<TmdbApiService>());

            var result = await service.SearchAsync(" ", MediaType.Movie);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Single(result.ValidationErrors);
            Assert.Equal(0, client.SearchCallCount);
        }

        [Fact]
        public async Task SearchAsync_Should_Include_Query_And_Normalized_Page_And_Map_Results()
        {
            var client = new FakeTmdbApiClient(
                searchResult: Result<TmdbSearchResponse>.Success(new TmdbSearchResponse
                {
                    Results =
                    [
                        new TmdbSearchResult { Id = 7, Title = "The Movie", PosterPath = "/movie.jpg" },
                        new TmdbSearchResult { Id = 8, Name = "Fallback Name", PosterPath = null }
                    ]
                }));

            var service = new TmdbApiService(client, ServiceTestLogger.Create<TmdbApiService>());

            var result = await service.SearchAsync("matrix", MediaType.Movie, page: 0);

            Assert.True(result.IsSuccess);
            Assert.Equal(MediaType.Movie, client.LastSearchMediaType);
            Assert.Equal(["query=matrix", "page=1"], client.LastSearchQueryParameters);
            Assert.Collection(
                result.Value,
                first => AssertSearchResult(first, "7", "The Movie", "https://image.tmdb.org/t/p/w500/movie.jpg", MediaType.Movie),
                second => AssertSearchResult(second, "8", "Fallback Name", null, MediaType.Movie));
        }

        [Fact]
        public async Task SearchAsync_Should_Propagate_ClientFailure()
        {
            var expectedError = MediaVaultErrors.Failure(new ErrorContext(
                operation: OperationType.GetCollection,
                entityName: "Tmdb"), "TMDB search failed.");

            var client = new FakeTmdbApiClient(
                searchResult: Result<TmdbSearchResponse>.Failure(expectedError, "TMDB search failed."));

            var service = new TmdbApiService(client, ServiceTestLogger.Create<TmdbApiService>());

            var result = await service.SearchAsync("matrix", MediaType.TvSeries);

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
            Assert.Equal("TMDB search failed.", result.Message);
        }

        private static void AssertSearchResult(
            MediaEntryExternalSearchResultDto result,
            string expectedIdExternal,
            string expectedTitle,
            string? expectedCoverImageUrl,
            MediaType expectedMediaType)
        {
            Assert.Equal(expectedIdExternal, result.IdExternal);
            Assert.Equal(expectedTitle, result.Title);
            Assert.Equal(expectedCoverImageUrl, result.CoverImageUrl);
            Assert.Equal(expectedMediaType, result.MediaType);
        }

        private sealed class FakeTmdbApiClient : ITmdbApiClient
        {
            private readonly Result<TmdbMovieDetailedResponse> _getMovieByIdResult;
            private readonly Result<TmdbTvSeriesDetailedResult> _getTvSeriesByIdResult;
            private readonly Result<TmdbSearchResponse> _searchResult;

            public FakeTmdbApiClient(
                Result<TmdbMovieDetailedResponse>? getMovieByIdResult = null,
                Result<TmdbTvSeriesDetailedResult>? getTvSeriesByIdResult = null,
                Result<TmdbSearchResponse>? searchResult = null)
            {
                _getMovieByIdResult = getMovieByIdResult ?? Result<TmdbMovieDetailedResponse>.Success(new TmdbMovieDetailedResponse());
                _getTvSeriesByIdResult = getTvSeriesByIdResult ?? Result<TmdbTvSeriesDetailedResult>.Success(new TmdbTvSeriesDetailedResult());
                _searchResult = searchResult ?? Result<TmdbSearchResponse>.Success(new TmdbSearchResponse { Results = [] });
            }

            public int GetMovieByIdCallCount { get; private set; }

            public int GetTvSeriesByIdCallCount { get; private set; }

            public int SearchCallCount { get; private set; }

            public IReadOnlyList<string>? LastSearchQueryParameters { get; private set; }

            public MediaType? LastSearchMediaType { get; private set; }

            public Task<Result<TmdbMovieDetailedResponse>> GetMovieByIdAsync(int id, CancellationToken cancellationToken = default)
            {
                GetMovieByIdCallCount++;
                return Task.FromResult(_getMovieByIdResult);
            }

            public Task<Result<TmdbTvSeriesDetailedResult>> GetTvSeriesByIdAsync(int id, CancellationToken cancellationToken = default)
            {
                GetTvSeriesByIdCallCount++;
                return Task.FromResult(_getTvSeriesByIdResult);
            }

            public Task<Result<TmdbSearchResponse>> SearchAsync(List<string> queryParameters, MediaType mediaType, CancellationToken cancellationToken = default)
            {
                SearchCallCount++;
                LastSearchQueryParameters = queryParameters.ToArray();
                LastSearchMediaType = mediaType;
                return Task.FromResult(_searchResult);
            }
        }
    }
}