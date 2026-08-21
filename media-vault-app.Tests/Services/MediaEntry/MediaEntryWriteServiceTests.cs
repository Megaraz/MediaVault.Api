using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.Mappers.MediaEntry;
using media_vault_app.Application.Services.MediaEntry;
using media_vault_app.Application.Validators.MediaEntry;
using media_vault_app.Domain.Enums;
using media_vault_app.Tests.TestHelpers;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;
using MovieEntryEntity = media_vault_app.Domain.Entities.MovieEntry;

namespace media_vault_app.Tests.Services.MediaEntry
{
    public class MediaEntryWriteServiceTests
    {
        [Fact]
        public async Task CreateAsync_Should_ReturnValidationFailure_When_OwnerIdAndDtoAreInvalid()
        {
            var mediaRepo = new FakeMediaEntryRepo();
            var ownerRepo = new FakeUserRepo();
            var service = CreateService(mediaRepo, ownerRepo);

            var result = await service.CreateAsync(Guid.Empty, new MovieEntryCreateDto { Title = "", Status = Status.Completed }, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Equal(2, result.ValidationErrors.Count);
            Assert.Equal(0, ownerRepo.ExistsCallCount);
            Assert.Equal(0, mediaRepo.CreateCallCount);
        }

        [Fact]
        public async Task CreateAsync_Should_RejectOutOfRangeRatingBeforeRepositoryWrites()
        {
            var mediaRepo = new FakeMediaEntryRepo();
            var ownerRepo = new FakeUserRepo();
            var service = CreateService(mediaRepo, ownerRepo);
            var dto = new MovieEntryCreateDto
            {
                Title = "Test Movie",
                Rating = 4.25m
            };

            var result = await service.CreateAsync(Guid.NewGuid(), dto, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Equal(0, ownerRepo.ExistsCallCount);
            Assert.Equal(0, mediaRepo.CreateCallCount);
        }

        [Fact]
        public async Task CreateAsync_Should_ReturnOwnerFailure_When_OwnerDoesNotExist()
        {
            var expectedError = MediaVaultErrors.NotFound(DefineErrorContext("CreateAsync", OperationType.Create));
            var ownerRepo = new FakeUserRepo
            {
                ExistsResult = Result<bool>.Failure(expectedError, "Owner not found.")
            };

            var mediaRepo = new FakeMediaEntryRepo();
            var service = CreateService(mediaRepo, ownerRepo);

            var result = await service.CreateAsync(Guid.NewGuid(), CreateMovieCreateDto(), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
            Assert.Equal(0, mediaRepo.CreateCallCount);
        }

        [Fact]
        public async Task CreateAsync_Should_SetOwnerId_And_ReturnMappedMovie_When_RequestIsValid()
        {
            var ownerId = Guid.NewGuid();
            var createdMovie = CreateMovie(ownerId: ownerId, title: "Created Movie");
            var mediaRepo = new FakeMediaEntryRepo
            {
                CreateResult = Result<MediaEntryEntity>.Success(createdMovie)
            };

            var service = CreateService(mediaRepo, new FakeUserRepo());

            var result = await service.CreateAsync(ownerId, CreateMovieCreateDto(title: "Created Movie"), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(mediaRepo.CreatedEntity);
            Assert.Equal(ownerId, mediaRepo.CreatedEntity!.OwnerId);
            Assert.Equal("Created Movie", mediaRepo.CreatedEntity.Title);
            Assert.Equal("Created Movie", result.Value.Title);
        }

        [Fact]
        public async Task UpdateAsync_Should_ReturnValidationFailure_When_OwnerId_DependencyId_And_DtoAreInvalid()
        {
            var service = CreateService(new FakeMediaEntryRepo(), new FakeUserRepo());

            var result = await service.UpdateAsync(Guid.Empty, Guid.Empty, new MovieEntryUpdateDto { ExpectedVersion = 1, Title = "", Status = Status.Completed }, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Equal(3, result.ValidationErrors.Count);
        }

        [Fact]
        public async Task UpdateAsync_Should_ReturnOwnerFailure_When_OwnerDoesNotExist()
        {
            var expectedError = MediaVaultErrors.NotFound(DefineErrorContext("UpdateAsync", OperationType.Update));
            var ownerRepo = new FakeUserRepo
            {
                ExistsResult = Result<bool>.Failure(expectedError, "Owner not found.")
            };

            var mediaRepo = new FakeMediaEntryRepo();
            var service = CreateService(mediaRepo, ownerRepo);

            var result = await service.UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), CreateMovieUpdateDto(), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
            Assert.Equal(0, mediaRepo.UpdateCallCount);
        }

        [Fact]
        public async Task UpdateAsync_Should_Map_Id_And_OwnerId_When_RequestIsValid()
        {
            var ownerId = Guid.NewGuid();
            var entryId = Guid.NewGuid();
            var mediaRepo = new FakeMediaEntryRepo();
            var service = CreateService(mediaRepo, new FakeUserRepo());

            var result = await service.UpdateAsync(ownerId, entryId, CreateMovieUpdateDto(title: "Updated Movie"), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(mediaRepo.UpdatedEntity);
            Assert.Equal(entryId, mediaRepo.UpdatedEntity!.Id);
            Assert.Equal(ownerId, mediaRepo.UpdatedEntity.OwnerId);
            Assert.Equal("Updated Movie", mediaRepo.UpdatedEntity.Title);
        }

        [Fact]
        public async Task DeleteAsync_Should_ReturnValidationFailure_When_OwnerIdAndDependentIdAreInvalid()
        {
            var mediaRepo = new FakeMediaEntryRepo();
            var service = CreateService(mediaRepo, new FakeUserRepo());

            var result = await service.DeleteAsync(Guid.Empty, Guid.Empty, 1, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Equal(2, result.ValidationErrors.Count);
            Assert.Equal(0, mediaRepo.DeleteCallCount);
        }

        [Fact]
        public async Task DeleteAsync_Should_Propagate_RepoFailure()
        {
            var expectedError = MediaVaultErrors.NotFound(DefineErrorContext("DeleteAsync", OperationType.Delete));
            var mediaRepo = new FakeMediaEntryRepo
            {
                DeleteResult = Result.Failure(expectedError, "Media entry not found.")
            };

            var service = CreateService(mediaRepo, new FakeUserRepo());
            var ownerId = Guid.NewGuid();
            var entryId = Guid.NewGuid();

            var result = await service.DeleteAsync(ownerId, entryId, 4, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
            Assert.Equal(ownerId, mediaRepo.LastOwnerId);
            Assert.Equal(entryId, mediaRepo.LastDependentId);
            Assert.Equal(4, mediaRepo.LastExpectedVersion);
        }

        private static MediaEntryWriteService CreateService(FakeMediaEntryRepo mediaRepo, FakeUserRepo ownerRepo)
        {
            return new MediaEntryWriteService(
                mediaRepo,
                ownerRepo,
                new MediaEntryEntityMapper(),
                new MediaEntryDtoMapper(),
                new MediaEntryDtoValidator(),
                ServiceTestLogger.Create<MediaEntryWriteService>());
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

        private static MovieEntryCreateDto CreateMovieCreateDto(string title = "Created Movie")
        {
            return new MovieEntryCreateDto
            {
                Title = title,
                Status = Status.Completed,
                Rating = 4.5m,
                ReleaseDate = new DateOnly(2025, 1, 1),
                RuntimeMinutes = 120
            };
        }

        private static MovieEntryUpdateDto CreateMovieUpdateDto(string title = "Updated Movie")
        {
            return new MovieEntryUpdateDto
            {
                ExpectedVersion = 3,
                Title = title,
                Status = Status.Completed,
                Rating = 4m,
                ReleaseDate = new DateOnly(2025, 2, 1),
                RuntimeMinutes = 110
            };
        }

        private static ErrorContext DefineErrorContext(string methodName, OperationType operation)
        {
            return new ErrorContext(
                operation: operation,
                entityName: "MediaEntry");
        }
    }
}
