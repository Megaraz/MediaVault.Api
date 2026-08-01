using media_vault_app.Application.DTOs.External_API_Contracts.GoogleBooks;
using media_vault_app.Application.DTOs.GoogleBooks;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Services.API;
using media_vault_app.Domain.Enums;
using media_vault_app.Tests.TestHelpers;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;

namespace media_vault_app.Tests.Services.API
{
    public class GoogleBooksApiServiceTests
    {
        [Fact]
        public async Task GetBookByIdAsync_Should_ReturnValidationFailure_When_VolumeIdIsBlank()
        {
            var client = new FakeGoogleBooksApiClient();
            var service = new GoogleBooksApiService(client, ServiceTestLogger.Create<GoogleBooksApiService>());

            var result = await service.GetBookByIdAsync(" ");

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Single(result.ValidationErrors);
            Assert.Equal(0, client.GetBookByIdCallCount);
        }

        [Fact]
        public async Task GetBookByIdAsync_Should_Map_Detailed_Response_And_Upgrade_Http_Thumbnail()
        {
            var client = new FakeGoogleBooksApiClient(
                getBookByIdResult: Result<GoogleBooksVolumeResponse>.Success(new GoogleBooksVolumeResponse(
                    Id: "volume-1",
                    VolumeInfo: new GoogleBooksVolumeInfo(
                        Title: "Clean Code",
                        Authors: ["Robert C. Martin", "Another Author"],
                        ImageLinks: new GoogleBooksImageLinks(
                            SmallThumbnail: null,
                            Thumbnail: "http://example.com/thumb.jpg",
                            Small: null,
                            Medium: null,
                            Large: null,
                            ExtraLarge: null)))));

            var service = new GoogleBooksApiService(client, ServiceTestLogger.Create<GoogleBooksApiService>());

            var result = await service.GetBookByIdAsync("volume-1");

            Assert.True(result.IsSuccess);
            Assert.Equal(new GoogleBooksDetailedDto(
                Author: "Robert C. Martin, Another Author",
                ExternalId: "volume-1",
                Title: "Clean Code",
                CoverImageUrl: "https://example.com/thumb.jpg",
                MediaType: MediaType.Book), result.Value);
            Assert.Equal(1, client.GetBookByIdCallCount);
        }

        [Fact]
        public async Task GetBookByIdAsync_Should_Propagate_ClientFailure()
        {
            var expectedError = MediaVaultErrors.NotFound(new ErrorContext(
                operation: OperationType.Get,
                entityName: "Google Books Volume"));

            var client = new FakeGoogleBooksApiClient(
                getBookByIdResult: Result<GoogleBooksVolumeResponse>.Failure(expectedError, "Book not found."));

            var service = new GoogleBooksApiService(client, ServiceTestLogger.Create<GoogleBooksApiService>());

            var result = await service.GetBookByIdAsync("missing-volume");

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
            Assert.Equal("Book not found.", result.Message);
        }

        [Fact]
        public async Task SearchBooksAsync_Should_ReturnValidationFailure_When_SearchIsBlank()
        {
            var client = new FakeGoogleBooksApiClient();
            var service = new GoogleBooksApiService(client, ServiceTestLogger.Create<GoogleBooksApiService>());

            var result = await service.SearchBooksAsync(" ");

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Single(result.ValidationErrors);
            Assert.Equal(0, client.SearchBooksCallCount);
        }

        [Fact]
        public async Task SearchBooksAsync_Should_NormalizePagination_And_Map_Results()
        {
            var client = new FakeGoogleBooksApiClient(
                searchBooksResult: Result<GoogleBooksSearchResponse>.Success(new GoogleBooksSearchResponse(
                    TotalItems: 2,
                    Items:
                    [
                        new GoogleBooksVolumeResponse(
                            Id: "volume-1",
                            VolumeInfo: new GoogleBooksVolumeInfo(
                                Title: "Refactoring",
                                Authors: ["Martin Fowler"],
                                ImageLinks: new GoogleBooksImageLinks(
                                    SmallThumbnail: null,
                                    Thumbnail: null,
                                    Small: "http://example.com/refactoring-small.jpg",
                                    Medium: null,
                                    Large: null,
                                    ExtraLarge: null))),
                        new GoogleBooksVolumeResponse(
                            Id: "volume-2",
                            VolumeInfo: new GoogleBooksVolumeInfo(
                                Title: null,
                                Authors: null,
                                ImageLinks: null))
                    ])));

            var service = new GoogleBooksApiService(client, ServiceTestLogger.Create<GoogleBooksApiService>());

            var result = await service.SearchBooksAsync("refactoring", page: 0, pageSize: 0);

            Assert.True(result.IsSuccess);
            Assert.Equal(["q=refactoring", "startIndex=0", "maxResults=1"], client.LastSearchQueryParameters);
            Assert.Collection(
                result.Value,
                first => Assert.Equal(new GoogleBooksDetailedDto("Martin Fowler", "volume-1", "Refactoring", "https://example.com/refactoring-small.jpg", MediaType.Book), first),
                second => Assert.Equal(new GoogleBooksDetailedDto("Unknown Author", "volume-2", string.Empty, null, MediaType.Book), second));
        }

        [Fact]
        public async Task SearchBooksAsync_Should_Propagate_ClientFailure()
        {
            var expectedError = MediaVaultErrors.Failure(new ErrorContext(
                operation: OperationType.GetCollection,
                entityName: "Google Books Volume"), "Search failed.");

            var client = new FakeGoogleBooksApiClient(
                searchBooksResult: Result<GoogleBooksSearchResponse>.Failure(expectedError, "Search failed."));

            var service = new GoogleBooksApiService(client, ServiceTestLogger.Create<GoogleBooksApiService>());

            var result = await service.SearchBooksAsync("ddd");

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
            Assert.Equal("Search failed.", result.Message);
        }

        private sealed class FakeGoogleBooksApiClient : IGoogleBooksApiClient
        {
            private readonly Result<GoogleBooksVolumeResponse> _getBookByIdResult;
            private readonly Result<GoogleBooksSearchResponse> _searchBooksResult;

            public FakeGoogleBooksApiClient(
                Result<GoogleBooksVolumeResponse>? getBookByIdResult = null,
                Result<GoogleBooksSearchResponse>? searchBooksResult = null)
            {
                _getBookByIdResult = getBookByIdResult ?? Result<GoogleBooksVolumeResponse>.Success(
                    new GoogleBooksVolumeResponse("default", null));
                _searchBooksResult = searchBooksResult ?? Result<GoogleBooksSearchResponse>.Success(
                    new GoogleBooksSearchResponse(0, []));
            }

            public int GetBookByIdCallCount { get; private set; }

            public int SearchBooksCallCount { get; private set; }

            public IReadOnlyList<string>? LastSearchQueryParameters { get; private set; }

            public Task<Result<GoogleBooksVolumeResponse>> GetBookByIdAsync(string volumeId, CancellationToken cancellationToken = default)
            {
                GetBookByIdCallCount++;
                return Task.FromResult(_getBookByIdResult);
            }

            public Task<Result<GoogleBooksSearchResponse>> SearchBooksAsync(List<string> queryParameters, CancellationToken cancellationToken = default)
            {
                SearchBooksCallCount++;
                LastSearchQueryParameters = queryParameters.ToArray();
                return Task.FromResult(_searchBooksResult);
            }
        }
    }
}