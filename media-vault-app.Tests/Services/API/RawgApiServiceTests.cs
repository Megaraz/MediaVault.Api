using media_vault_app.Application.DTOs.External_API_Contracts.Rawg;
using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Services.API;
using media_vault_app.Domain.Enums;
using media_vault_app.Tests.TestHelpers;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;

namespace media_vault_app.Tests.Services.API
{
    public class RawgApiServiceTests
    {
        [Fact]
        public async Task GetGameByIdAsync_Should_Map_Detailed_Response_To_Dto()
        {
            // Arrange
            var client = new FakeRawgApiClient(
                Result<RawgGameDetailedResponse>.Success(new RawgGameDetailedResponse
                {
                    Id = 42,
                    Slug = "test-game",
                    Name = "Test Game",
                    Description = "Test description",
                    Metacritic = 88,
                    Released = "2024-01-01",
                    BackgroundImage = "https://example.com/image.jpg",
                    Website = "https://example.com",
                    Platforms =
                    [
                        new Platform
                        {
                            Platform1 = new Platform1 { Id = 1, Name = "PC", Slug = "pc" },
                            Requirements = new Requirements
                            {
                                Minimum = "Minimum specs",
                                Recommended = "Recommended specs"
                            }
                        },
                        new Platform
                        {
                            Platform1 = new Platform1 { Id = 2, Name = "PlayStation 5", Slug = "playstation5" }
                        },
                        new Platform
                        {
                            Platform1 = new Platform1 { Id = 3, Name = "PC", Slug = "pc" }
                        }
                    ]
                }));

            var service = new RawgApiService(client, ServiceTestLogger.Create<RawgApiService>());

            // Act
            var result = await service.GetGameByIdAsync(42);

            // Assert
            Assert.True(result.IsSuccess);

            var dto = result.Value;
            Assert.Equal(42, dto.RawgId);
            Assert.Equal("test-game", dto.RawgSlug);
            Assert.Equal("Test Game", dto.RawgName);
            Assert.Equal("Test description", dto.RawgDescription);
            Assert.Equal(88, dto.RawgMetacritic);
            Assert.Equal("2024-01-01", dto.RawgReleased);
            Assert.Equal("https://example.com/image.jpg", dto.RawgBackgroundImage);
            Assert.Equal("https://example.com", dto.RawgWebsite);
            Assert.Equal(["PC", "PlayStation 5"], dto.RawgPlatforms);
            Assert.NotNull(dto.RawgRequirements);
            Assert.Equal("Minimum specs", dto.RawgRequirements!.Minimum);
            Assert.Equal("Recommended specs", dto.RawgRequirements.Recommended);
            Assert.Null(dto.RawgRequirements.High);
            Assert.Null(dto.RawgRequirements.VeryHigh);
            Assert.Null(dto.RawgRequirements.Ultra);
        }

        [Fact]
        public async Task GetGameByIdAsync_Should_ReturnValidationFailure_When_IdIsInvalid()
        {
            var client = new FakeRawgApiClient();
            var service = new RawgApiService(client, ServiceTestLogger.Create<RawgApiService>());

            var result = await service.GetGameByIdAsync(0);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Single(result.ValidationErrors);
            Assert.Equal(0, client.GetGameByIdCallCount);
        }

        [Fact]
        public async Task GetGameByIdAsync_Should_Propagate_ClientFailure()
        {
            var expectedError = MediaVaultErrors.NotFound(new ErrorContext(
                operation: OperationType.Get,
                entityName: "Rawg Game"));

            var client = new FakeRawgApiClient(
                getGameByIdResult: Result<RawgGameDetailedResponse>.Failure(expectedError, "RAWG game was not found."));

            var service = new RawgApiService(client, ServiceTestLogger.Create<RawgApiService>());

            var result = await service.GetGameByIdAsync(42);

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
            Assert.Equal("RAWG game was not found.", result.Message);
            Assert.Equal(1, client.GetGameByIdCallCount);
        }

        [Fact]
        public async Task GetGameByIdAsync_Should_Map_FirstAvailableRequirements_When_PcRequirementsAreMissing()
        {
            var client = new FakeRawgApiClient(
                getGameByIdResult: Result<RawgGameDetailedResponse>.Success(new RawgGameDetailedResponse
                {
                    Id = 5,
                    Name = "Fallback Requirements Game",
                    Platforms =
                    [
                        new Platform
                        {
                            Platform1 = new Platform1 { Id = 1, Name = "PC", Slug = "pc" },
                            Requirements = new Requirements { Minimum = " ", Recommended = null }
                        },
                        new Platform
                        {
                            Platform1 = new Platform1 { Id = 2, Name = "Xbox Series S/X", Slug = "xbox-series-s-x" },
                            Requirements = new Requirements { Minimum = "Console minimum", Recommended = "Console recommended" }
                        }
                    ]
                }));

            var service = new RawgApiService(client, ServiceTestLogger.Create<RawgApiService>());

            var result = await service.GetGameByIdAsync(5);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.RawgRequirements);
            Assert.Equal("Console minimum", result.Value.RawgRequirements!.Minimum);
            Assert.Equal("Console recommended", result.Value.RawgRequirements.Recommended);
        }

        [Fact]
        public async Task SearchGamesAsync_Should_ReturnValidationFailure_When_SearchIsBlank()
        {
            var client = new FakeRawgApiClient();
            var service = new RawgApiService(client, ServiceTestLogger.Create<RawgApiService>());

            var result = await service.SearchGamesAsync(" ");

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Single(result.ValidationErrors);
            Assert.Equal(0, client.SearchGamesCallCount);
        }

        [Fact]
        public async Task SearchGamesAsync_Should_NormalizePagination_IncludeOptionalParameters_And_MapResults()
        {
            var client = new FakeRawgApiClient(
                searchGamesResult: Result<RawgSearchResponse>.Success(new RawgSearchResponse(
                [
                    new RawgGameSearchResult(10, "elden-ring", "Elden Ring", "https://example.com/elden-ring.jpg"),
                    new RawgGameSearchResult(11, "", null, null)
                ])));

            var service = new RawgApiService(client, ServiceTestLogger.Create<RawgApiService>());

            var result = await service.SearchGamesAsync(
                "Elden Ring",
                page: -3,
                pageSize: 0,
                searchPrecise: true,
                searchExact: false,
                ordering: "-rating");

            Assert.True(result.IsSuccess);
            Assert.Equal(1, client.SearchGamesCallCount);
            Assert.Equal(
                [
                    "search=Elden%20Ring",
                    "page=1",
                    "page_size=1",
                    "search_precise=true",
                    "search_exact=false",
                    "ordering=-rating"
                ],
                client.LastSearchQueryParameters);

            var mappedResults = result.Value;
            Assert.Collection(
                mappedResults,
                first => AssertSearchResult(first, "10", "Elden Ring", "https://example.com/elden-ring.jpg", MediaType.Game),
                second => AssertSearchResult(second, "11", string.Empty, null, MediaType.Game));
        }

        [Fact]
        public async Task SearchGamesAsync_Should_Propagate_ClientFailure()
        {
            var expectedError = MediaVaultErrors.Failure(new ErrorContext(
                operation: OperationType.GetCollection,
                entityName: "Rawg Game"), "RAWG search failed.");

            var client = new FakeRawgApiClient(
                searchGamesResult: Result<RawgSearchResponse>.Failure(expectedError, "RAWG search failed."));

            var service = new RawgApiService(client, ServiceTestLogger.Create<RawgApiService>());

            var result = await service.SearchGamesAsync("Halo");

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
            Assert.Equal("RAWG search failed.", result.Message);
            Assert.Equal(1, client.SearchGamesCallCount);
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

        private sealed class FakeRawgApiClient : IRawgApiClient
        {
            private readonly Result<RawgGameDetailedResponse> _gameByIdResult;
            private readonly Result<RawgSearchResponse> _searchGamesResult;

            public FakeRawgApiClient(
                Result<RawgGameDetailedResponse>? getGameByIdResult = null,
                Result<RawgSearchResponse>? searchGamesResult = null)
            {
                _gameByIdResult = getGameByIdResult ?? Result<RawgGameDetailedResponse>.Success(new RawgGameDetailedResponse());
                _searchGamesResult = searchGamesResult ?? Result<RawgSearchResponse>.Success(new RawgSearchResponse([]));
            }

            public int GetGameByIdCallCount { get; private set; }

            public int SearchGamesCallCount { get; private set; }

            public IReadOnlyList<string>? LastSearchQueryParameters { get; private set; }

            public Task<Result<RawgGameDetailedResponse>> GetGameByIdAsync(int id, CancellationToken cancellationToken = default)
            {
                GetGameByIdCallCount++;
                return Task.FromResult(_gameByIdResult);
            }

            public Task<Result<RawgSearchResponse>> SearchGamesAsync(List<string> queryParameters, CancellationToken cancellationToken = default)
            {
                SearchGamesCallCount++;
                LastSearchQueryParameters = queryParameters.ToArray();
                return Task.FromResult(_searchGamesResult);
            }
        }
    }
}
