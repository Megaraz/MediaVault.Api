using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Mappers.User;
using media_vault_app.Application.Services.User;
using media_vault_app.Application.Validators.User;
using media_vault_app.Tests.TestHelpers;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Tests.Services.User
{
    public class UserWriteServiceTests
    {
        [Fact]
        public async Task CreateAsync_Should_ReturnValidationFailure_When_DtoIsInvalid()
        {
            var userRepo = new FakeUserRepo();
            var service = CreateService(userRepo);

            var result = await service.CreateAsync(new UserRegisterDto("", "mail@example.com", "mail@example.com", "Password123", "Password123"), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Equal(0, userRepo.CreateCallCount);
        }

        [Fact]
        public async Task CreateAsync_Should_RejectInvalidEmailBeforeRepositoryWrites()
        {
            var userRepo = new FakeUserRepo();
            var service = CreateService(userRepo);
            var dto = new UserRegisterDto(
                "valid-user",
                "not-an-email",
                "not-an-email",
                "Password123",
                "Password123");

            var result = await service.CreateAsync(dto, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Equal(0, userRepo.CreateCallCount);
        }

        [Fact]
        public async Task CreateAsync_Should_Map_And_Create_User_When_DtoIsValid()
        {
            var createdUser = new UserEntity
            {
                Id = Guid.NewGuid(),
                Username = "created-user",
                Email = "created@example.com",
                PasswordHash = "stored-hash",
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            var userRepo = new FakeUserRepo
            {
                CreateResult = Result<UserEntity>.Success(createdUser)
            };

            var service = CreateService(userRepo);
            var dto = CreateRegisterDto();

            var result = await service.CreateAsync(dto, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(userRepo.CreatedEntity);
            Assert.Equal(dto.Username, userRepo.CreatedEntity!.Username);
            Assert.Equal(dto.Email, userRepo.CreatedEntity.Email);
            Assert.Equal(dto.Password, userRepo.CreatedEntity.PasswordHash);
            Assert.Equal(createdUser.Id, result.Value.Id);
        }

        [Fact]
        public async Task CreateAsync_Should_Propagate_RepoFailure()
        {
            var expectedError = MediaVaultErrors.Conflict(DefineErrorContext("CreateAsync", OperationType.Create));
            var userRepo = new FakeUserRepo
            {
                CreateResult = Result<UserEntity>.Failure(expectedError, "User already exists.")
            };

            var service = CreateService(userRepo);

            var result = await service.CreateAsync(CreateRegisterDto(), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
        }

        [Fact]
        public async Task UpdateAsync_Should_ReturnCombinedValidationFailure_When_IdAndDtoAreInvalid()
        {
            var userRepo = new FakeUserRepo();
            var service = CreateService(userRepo);

            var result = await service.UpdateAsync(Guid.Empty, new UserUpdateDto { ExpectedVersion = 1, UserName = "", Email = " " }, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Equal(3, result.ValidationErrors.Count);
            Assert.Equal(0, userRepo.UpdateCallCount);
        }

        [Fact]
        public async Task UpdateAsync_Should_Map_Id_And_Dto_When_RequestIsValid()
        {
            var userRepo = new FakeUserRepo();
            var service = CreateService(userRepo);
            var userId = Guid.NewGuid();

            var result = await service.UpdateAsync(
                userId,
                new UserUpdateDto
                {
                    ExpectedVersion = 1,
                    UserName = " Updated-User ",
                    Email = " UPDATED@Example.COM "
                },
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(userRepo.UpdatedEntity);
            Assert.Equal(userId, userRepo.UpdatedEntity!.Id);
            Assert.Equal("updated-user", userRepo.UpdatedEntity.Username);
            Assert.Equal("updated@example.com", userRepo.UpdatedEntity.Email);
        }

        [Fact]
        public async Task UpdateProfileAsync_Should_UseNormalizedValues_AndDedicatedRepositoryPath()
        {
            var userRepo = new FakeUserRepo();
            var service = CreateService(userRepo);
            var userId = Guid.NewGuid();

            var result = await service.UpdateProfileAsync(
                userId,
                new UserUpdateDto
                {
                    ExpectedVersion = 7,
                    UserName = " Updated-User ",
                    Email = " UPDATED@Example.COM "
                },
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(1, userRepo.ProfileAvailabilityCallCount);
            Assert.Equal(1, userRepo.ProfileUpdateCallCount);
            Assert.Equal((userId, "updated-user", "updated@example.com"), userRepo.LastProfileUpdateAvailabilityRequest);
            Assert.Equal((userId, "updated-user", "updated@example.com", 7), userRepo.LastProfileUpdateRequest);
            Assert.Equal(0, userRepo.UpdateCallCount);
        }

        [Theory]
        [InlineData(false, true, "UserName")]
        [InlineData(true, false, "Email")]
        public async Task UpdateProfileAsync_Should_RejectNormalizedDuplicateValues(
            bool isUsernameAvailable,
            bool isEmailAvailable,
            string expectedFieldName)
        {
            var userRepo = new FakeUserRepo
            {
                ProfileAvailabilityResult = Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.Success(
                    (isUsernameAvailable, isEmailAvailable))
            };
            var service = CreateService(userRepo);

            var result = await service.UpdateProfileAsync(
                Guid.NewGuid(),
                new UserUpdateDto
                {
                    ExpectedVersion = 1,
                    UserName = " conflicting-user ",
                    Email = " conflicting@example.com "
                },
                CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            var error = Assert.Single(result.ValidationErrors);
            Assert.Equal(expectedFieldName, error.FieldName);
            Assert.Equal(0, userRepo.ProfileUpdateCallCount);
            Assert.Equal(
                ("conflicting-user", "conflicting@example.com"),
                (
                    userRepo.LastProfileUpdateAvailabilityRequest!.Value.Username,
                    userRepo.LastProfileUpdateAvailabilityRequest.Value.Email));
        }

        [Fact]
        public async Task UpdateProfileAsync_Should_AllowSameNormalizedValues_ForCurrentUser()
        {
            var userRepo = new FakeUserRepo
            {
                ProfileAvailabilityResult = Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.Success((true, true))
            };
            var service = CreateService(userRepo);
            var userId = Guid.NewGuid();

            var result = await service.UpdateProfileAsync(
                userId,
                new UserUpdateDto
                {
                    ExpectedVersion = 3,
                    UserName = " current-user ",
                    Email = " current@example.com "
                },
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal((userId, "current-user", "current@example.com", 3), userRepo.LastProfileUpdateRequest);
        }

        [Fact]
        public async Task DeleteAsync_Should_ReturnValidationFailure_When_IdIsInvalid()
        {
            var userRepo = new FakeUserRepo();
            var service = CreateService(userRepo);

            var result = await service.DeleteAsync(Guid.Empty, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Equal(0, userRepo.DeleteCallCount);
        }

        [Fact]
        public async Task DeleteAsync_Should_Propagate_RepoFailure()
        {
            var expectedError = MediaVaultErrors.NotFound(DefineErrorContext("DeleteAsync", OperationType.Delete));
            var userRepo = new FakeUserRepo
            {
                DeleteResult = Result.Failure(expectedError, "User not found.")
            };

            var service = CreateService(userRepo);

            var result = await service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
        }

        private static UserWriteService CreateService(FakeUserRepo userRepo)
        {
            return new UserWriteService(
                userRepo,
                new UserEntityMapper(),
                new UserDtoMapper(),
                new UserDtoValidator(),
                ServiceTestLogger.Create<UserWriteService>());
        }

        private static UserRegisterDto CreateRegisterDto()
        {
            return new UserRegisterDto("created-user", "created@example.com", "created@example.com", "Password123", "Password123");
        }

        private static ErrorContext DefineErrorContext(string methodName, OperationType operation)
        {
            return new ErrorContext(
                operation: operation,
                entityName: "User");
        }
    }
}
