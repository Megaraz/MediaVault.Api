using media_vault_app.Application.Mappers.User;
using media_vault_app.Application.Services.User;
using media_vault_app.Tests.TestHelpers;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Tests.Services.User;

public sealed class UserReadServiceTests
{
    [Fact]
    public async Task GetCurrentUserAsync_Should_ReturnValidationFailure_When_IdIsEmpty()
    {
        var userRepo = new FakeUserRepo();
        var service = CreateService(userRepo);

        var result = await service.GetCurrentUserAsync(Guid.Empty, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
        Assert.Single(result.ValidationErrors);
        Assert.Equal(0, userRepo.GetByIdCallCount);
    }

    [Fact]
    public async Task GetCurrentUserAsync_Should_Map_User_When_RepoReturnsUser()
    {
        var user = CreateUser();
        var userRepo = new FakeUserRepo
        {
            GetByIdResult = Result<UserEntity>.Success(user)
        };
        var service = CreateService(userRepo);

        var result = await service.GetCurrentUserAsync(user.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value.Id);
        Assert.Equal(user.Username, result.Value.Username);
        Assert.Equal(user.Email, result.Value.Email);
        Assert.Equal(1, userRepo.GetByIdCallCount);
    }

    [Fact]
    public async Task GetCurrentUserAsync_Should_Propagate_RepoFailure()
    {
        var expectedError = MediaVaultErrors.NotFound(DefineErrorContext());
        var userRepo = new FakeUserRepo
        {
            GetByIdResult = Result<UserEntity>.Failure(expectedError, "User not found.")
        };
        var service = CreateService(userRepo);

        var result = await service.GetCurrentUserAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.PrimaryError);
        Assert.Equal("User not found.", result.Message);
    }

    private static UserReadService CreateService(FakeUserRepo userRepo) =>
        new(userRepo, ServiceTestLogger.Create<UserReadService>());

    private static UserEntity CreateUser(string username = "testuser") =>
        new()
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = $"{username}@example.com",
            CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

    private static ErrorContext DefineErrorContext() =>
        new(operation: OperationType.Get, entityName: "User");
}
