using media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search;
using media_vault_app.Application.Mappers.MediaEntry;
using media_vault_app.Application.Services.MediaEntry;
using media_vault_app.Domain.Enums;
using media_vault_app.Tests.TestHelpers;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using BookEntryEntity = media_vault_app.Domain.Entities.BookEntry;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;
using MovieEntryEntity = media_vault_app.Domain.Entities.MovieEntry;

namespace media_vault_app.Tests.Services.MediaEntry
{
    using media_vault_app.Application.DTOs.MediaEntry.Response;

    public class MediaEntryReadServiceTests
    {
        [Fact]
        public async Task GetMovieByIdAsync_Should_ReturnValidationFailure_When_OwnerIdAndIdAreInvalid()
        {
            var mediaRepo = new FakeMediaEntryRepo();
            var ownerRepo = new FakeUserRepo();
            var service = CreateService(mediaRepo, ownerRepo);

            var result = await service.GetMovieByIdAsync(Guid.Empty, Guid.Empty, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Equal(2, result.ValidationErrors.Count);
            Assert.Equal(0, ownerRepo.ExistsCallCount);
            Assert.Equal(0, mediaRepo.GetByIdCallCount);
        }

        [Fact]
        public async Task GetMovieByIdAsync_Should_ReturnTypedMovie_When_RepoReturnsMovie()
        {
            var ownerId = Guid.NewGuid();
            var movie = CreateMovie(ownerId: ownerId, title: "Alien");
            var mediaRepo = new FakeMediaEntryRepo
            {
                GetByIdResult = Result<MediaEntryEntity>.Success(movie)
            };

            var ownerRepo = new FakeUserRepo();
            var service = CreateService(mediaRepo, ownerRepo);

            var result = await service.GetMovieByIdAsync(ownerId, movie.Id, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(movie.Id, result.Value.Id);
            Assert.Equal("Alien", result.Value.Title);
            Assert.Equal(MediaType.Movie, result.Value.MediaType);
        }

        [Fact]
        public async Task GetMovieByIdAsync_Should_ReturnNotFound_When_MediaTypeDoesNotMatch()
        {
            var ownerId = Guid.NewGuid();
            var mediaRepo = new FakeMediaEntryRepo
            {
                GetByIdResult = Result<MediaEntryEntity>.Success(CreateBook(ownerId: ownerId))
            };

            var service = CreateService(mediaRepo, new FakeUserRepo());

            var result = await service.GetMovieByIdAsync(ownerId, Guid.NewGuid(), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.PrimaryError.Type);
        }

        [Fact]
        public async Task GetDetailedCollectionByOwnerIdAsync_Should_ReturnOwnerFailure_When_OwnerDoesNotExist()
        {
            var expectedError = MediaVaultErrors.NotFound(DefineErrorContext("GetDetailedCollectionByOwnerIdAsync", OperationType.GetCollection));
            var ownerRepo = new FakeUserRepo
            {
                ExistsResult = Result<bool>.Failure(expectedError, "Owner not found.")
            };

            var mediaRepo = new FakeMediaEntryRepo();
            var service = CreateService(mediaRepo, ownerRepo);

            var result = await service.GetDetailedCollectionByOwnerIdAsync(Guid.NewGuid(), ct: CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
            Assert.Equal(0, mediaRepo.GetCollectionByOwnerIdCallCount);
        }

        [Fact]
        public async Task GetMinimalCollectionByOwnerIdAsync_Should_NormalizePagination_And_Map_Collection()
        {
            var ownerId = Guid.NewGuid();
            var mediaRepo = new FakeMediaEntryRepo
            {
                MinimalCollectionByOwnerIdResult = Result<IReadOnlyList<MediaEntryMinimalDto>>.Success([CreateMinimalDto(title: "Movie One")])
            };

            var service = CreateService(mediaRepo, new FakeUserRepo());

            var result = await service.GetMinimalCollectionByOwnerIdAsync(ownerId, pageNumber: 0, pageSize: 0, ct: CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(1, mediaRepo.GetMinimalCollectionByOwnerIdCallCount);
            Assert.Equal((ownerId, 1, 1), mediaRepo.LastCollectionRequest);
            Assert.Equal("Movie One", Assert.Single(result.Value).Title);
        }

        [Fact]
        public async Task SearchMediaEntriesAsync_Should_ReturnValidationFailure_When_OwnerIdAndQueryAreInvalid()
        {
            var service = CreateService(new FakeMediaEntryRepo(), new FakeUserRepo());

            var result = await service.SearchMediaEntriesAsync(Guid.Empty, new SearchRequestDto(" ", 4), ct: CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Equal(2, result.ValidationErrors.Count);
        }

        [Fact]
        public async Task SearchMediaEntriesAsync_Should_NormalizePagination_And_Map_Results()
        {
            var ownerId = Guid.NewGuid();
            var mediaRepo = new FakeMediaEntryRepo
            {
                SearchMediaEntriesResult = Result<IReadOnlyList<MediaEntryMinimalDto>>.Success([CreateMinimalDto(title: "The Matrix")])
            };

            var service = CreateService(mediaRepo, new FakeUserRepo());

            var result = await service.SearchMediaEntriesAsync(ownerId, new SearchRequestDto("matrix", 9), pageNumber: 0, pageSize: 0, ct: CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal((ownerId, "matrix", 1, 1), mediaRepo.LastSearchRequest);
            Assert.Equal("The Matrix", Assert.Single(result.Value).Title);
        }

        private static MediaEntryReadService CreateService(FakeMediaEntryRepo mediaRepo, FakeUserRepo ownerRepo)
        {
            return new MediaEntryReadService(mediaRepo, ownerRepo, new MediaEntryEntityMapper(), ServiceTestLogger.Create<MediaEntryReadService>());
        }

        private static MovieEntryEntity CreateMovie(Guid? ownerId = null, Guid? id = null, string title = "Test Movie")
        {
            return new MovieEntryEntity
            {
                Id = id ?? Guid.NewGuid(),
                OwnerId = ownerId ?? Guid.NewGuid(),
                Title = title,
                Status = Status.Completed,
                Rating = 4.5m,
                ReleaseDate = new DateOnly(2025, 1, 1),
                RuntimeMinutes = 120,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };
        }

        private static BookEntryEntity CreateBook(Guid? ownerId = null)
        {
            return new BookEntryEntity
            {
                Id = Guid.NewGuid(),
                OwnerId = ownerId ?? Guid.NewGuid(),
                Title = "A Book",
                Status = Status.Completed,
                Rating = 4m,
                ReleaseDate = new DateOnly(2025, 1, 1),
                Author = "Author",
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };
        }

        private static MediaEntryMinimalDto CreateMinimalDto(string title) =>
            new()
            {
                Id = Guid.NewGuid(),
                Title = title,
                Status = Status.Completed,
                Rating = 4.5m,
                ReleaseDate = new DateOnly(2025, 1, 1),
                MediaType = MediaType.Movie,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

        private static ErrorContext DefineErrorContext(string methodName, OperationType operation)
        {
            return new ErrorContext(
                operation: operation,
                entityName: "MediaEntry");
        }
    }
}
