using media_vault_app.Application.Mappers.User;
using media_vault_app.Application.Services.User;
using media_vault_app.Tests.TestHelpers;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Tests.Services.User
{
    public class UserReadServiceTests
    {
        [Fact]
        public async Task GetByIdAsync_Should_ReturnValidationFailure_When_IdIsEmpty()
        {
            var userRepo = new FakeUserRepo();
            var service = CreateService(userRepo);

            var result = await service.GetByIdAsync(Guid.Empty, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Single(result.ValidationErrors);
            Assert.Equal(0, userRepo.GetByIdCallCount);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Map_User_When_RepoReturnsUser()
        {
            var user = CreateUser();
            var userRepo = new FakeUserRepo
            {
                GetByIdResult = Result<UserEntity>.Success(user)
            };

            var service = CreateService(userRepo);

            var result = await service.GetByIdAsync(user.Id, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(user.Id, result.Value.Id);
            Assert.Equal(user.Username, result.Value.Username);
            Assert.Equal(user.Email, result.Value.Email);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Propagate_RepoFailure()
        {
            var expectedError = MediaVaultErrors.NotFound(DefineErrorContext("GetByIdAsync", OperationType.Get));
            var userRepo = new FakeUserRepo
            {
                GetByIdResult = Result<UserEntity>.Failure(expectedError, "User not found.")
            };

            var service = CreateService(userRepo);

            var result = await service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
        }

        [Fact]
        public async Task GetDetailedCollectionAsync_Should_NormalizePagination_And_Map_Users()
        {
            var users = new[] { CreateUser(username: "alice"), CreateUser(username: "bob") };
            var userRepo = new FakeUserRepo
            {
                GetCollectionResult = Result<IReadOnlyList<UserEntity>>.Success(users)
            };

            var service = CreateService(userRepo);

            var result = await service.GetDetailedCollectionAsync(pageNumber: 0, pageSize: 0, ct: CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal((1, 1), userRepo.LastCollectionRequest);
            Assert.Equal(["alice", "bob"], result.Value.Select(user => user.Username).ToArray());
        }

        [Fact]
        public async Task GetMinimalCollectionAsync_Should_Propagate_RepoFailure()
        {
            var expectedError = MediaVaultErrors.Failure(DefineErrorContext("GetMinimalCollectionAsync", OperationType.GetCollection), "Collection failed.");
            var userRepo = new FakeUserRepo
            {
                GetCollectionResult = Result<IReadOnlyList<UserEntity>>.Failure(expectedError, "Collection failed.")
            };

            var service = CreateService(userRepo);

            var result = await service.GetMinimalCollectionAsync(ct: CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
        }

        private static UserReadService CreateService(FakeUserRepo userRepo)
        {
            return new UserReadService(userRepo, new UserEntityMapper(), ServiceTestLogger.Create<UserReadService>());
        }

        private static UserEntity CreateUser(string username = "testuser")
        {
            return new UserEntity
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = $"{username}@example.com",
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };
        }

        private static ErrorContext DefineErrorContext(string methodName, OperationType operation)
        {
            return new ErrorContext(
                operation: operation,
                entityName: "User");
        }
    }
}