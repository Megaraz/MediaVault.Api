using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Services.User;
using media_vault_app.Application.Validators.User;
using media_vault_app.Tests.TestHelpers;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;

namespace media_vault_app.Tests.Services.User;

public sealed class UserWriteServiceTests
{
    [Fact]
    public async Task UpdateProfileAsync_Should_ReturnCombinedValidationFailure_When_IdAndDtoAreInvalid()
    {
        var userRepo = new FakeUserRepo();
        var service = CreateService(userRepo);

        var result = await service.UpdateProfileAsync(
            Guid.Empty,
            new UserUpdateDto { ExpectedVersion = 1, UserName = "", Email = " " },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
        Assert.Equal(3, result.ValidationErrors.Count);
        Assert.Equal(0, userRepo.ProfileAvailabilityCallCount);
    }

    [Fact]
    public async Task UpdateProfileAsync_Should_UseNormalizedValues_And_DedicatedRepositoryPath()
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
        Assert.Equal((userId, "updated-user", "updated@example.com"), userRepo.LastProfileUpdateAvailabilityRequest);
        Assert.Equal((userId, "updated-user", "updated@example.com", 7), userRepo.LastProfileUpdateRequest);
        Assert.Equal(1, userRepo.ProfileUpdateCallCount);
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
        Assert.Equal(expectedFieldName, Assert.Single(result.ValidationErrors).FieldName);
        Assert.Equal(0, userRepo.ProfileUpdateCallCount);
    }

    [Fact]
    public async Task DeleteOwnAccountAsync_Should_ReturnValidationFailure_When_IdIsInvalid()
    {
        var userRepo = new FakeUserRepo();
        var service = CreateService(userRepo);

        var result = await service.DeleteOwnAccountAsync(Guid.Empty, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
        Assert.Equal(0, userRepo.DeleteAccountCallCount);
    }

    [Fact]
    public async Task DeleteOwnAccountAsync_Should_UseAuthenticatedUserId_And_PropagateRepositoryResult()
    {
        var expectedError = MediaVaultErrors.NotFound(
            new(operation: OperationType.Delete, entityName: "User"));
        var userRepo = new FakeUserRepo
        {
            DeleteAccountResult = Result.Failure(expectedError, "User not found.")
        };
        var service = CreateService(userRepo);
        var userId = Guid.NewGuid();

        var result = await service.DeleteOwnAccountAsync(userId, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.PrimaryError);
        Assert.Equal("User not found.", result.Message);
        Assert.Equal(1, userRepo.DeleteAccountCallCount);
        Assert.Equal(userId, userRepo.DeletedId);
    }

    private static UserWriteService CreateService(FakeUserRepo userRepo) =>
        new(userRepo, new UserDtoValidator(), ServiceTestLogger.Create<UserWriteService>());
}
