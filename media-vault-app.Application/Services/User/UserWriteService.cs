using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Identity;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Interfaces.Validators;
using Microsoft.Extensions.Logging;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using media_vault_app.Application.Results;
using media_vault_app.Application.Validation;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Services.User;

public class UserWriteService : IUserWriteService
{
    private readonly IUserRepo _userRepo;
    private readonly IUserDtoValidator _dtoValidator;
    private readonly ILogger<UserWriteService> _logger;

    public UserWriteService(
        IUserRepo userRepo,
        IUserDtoValidator validator,
        ILogger<UserWriteService> logger)
    {
        _userRepo = userRepo;
        _dtoValidator = validator;
        _logger = logger;
    }

    public async Task<Result> UpdateProfileAsync(
        Guid userId,
        UserUpdateDto updateDto,
        CancellationToken ct = default)
    {
        var baseErrorContext = DefineErrorContext(nameof(UpdateProfileAsync), OperationType.Update);
        var validationErrors = new List<ValidationError>();
        var canonicalUpdateDto = updateDto is null
            ? null
            : UserIdentifierCanonicalizer.Canonicalize(updateDto);

        if (userId.IsNotValidMediaVaultId(baseErrorContext with { FieldName = nameof(userId) }, out var userIdError))
            validationErrors.Add(userIdError);

        if (!_dtoValidator.IsValidUpdateDto(canonicalUpdateDto!, baseErrorContext, out var updateValidationErrors))
            validationErrors.AddRange(updateValidationErrors);

        if (validationErrors.Count > 0)
        {
            ServiceValidationLogging.LogValidationFailure(
                _logger, validationErrors, GetType().Name, nameof(UpdateProfileAsync), baseErrorContext);
            return Result.ValidationFailure(validationErrors, MediaVaultResultMessages.ValidationFailure);
        }

        var username = canonicalUpdateDto!.UserName;
        var email = canonicalUpdateDto.Email;
        var availabilityResult = await _userRepo.CheckProfileUpdateAvailabilityAsync(
            userId, username, email, ct);

        if (availabilityResult.IsFailure)
        {
            if (availabilityResult.PrimaryError.Type != ErrorType.Validation)
                return availabilityResult;

            ServiceValidationLogging.LogValidationFailure(
                _logger,
                availabilityResult.ValidationErrors,
                GetType().Name,
                nameof(UpdateProfileAsync),
                baseErrorContext);
            return Result.ValidationFailure(
                availabilityResult.ValidationErrors,
                "User profile update validation failed.");
        }

        var profileValidationErrors = new List<ValidationError>();

        if (!availabilityResult.Value.IsUserNameAvailable)
        {
            profileValidationErrors.Add(
                MediaVaultValidationError.AlreadyExists(
                    baseErrorContext with { FieldName = nameof(UserUpdateDto.UserName) }));
        }

        if (!availabilityResult.Value.IsEmailAvailable)
        {
            profileValidationErrors.Add(
                MediaVaultValidationError.AlreadyExists(
                    baseErrorContext with { FieldName = nameof(UserUpdateDto.Email) }));
        }

        if (profileValidationErrors.Count > 0)
        {
            ServiceValidationLogging.LogValidationFailure(
                _logger,
                profileValidationErrors,
                GetType().Name,
                nameof(UpdateProfileAsync),
                baseErrorContext);
            return Result.ValidationFailure(
                profileValidationErrors,
                ProfileAvailabilityMessage(
                    !availabilityResult.Value.IsUserNameAvailable,
                    !availabilityResult.Value.IsEmailAvailable));
        }

        return await _userRepo.UpdateProfileAsync(
            userId,
            username,
            email,
            canonicalUpdateDto.ExpectedVersion,
            ct);
    }

    public async Task<Result> DeleteOwnAccountAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(DeleteOwnAccountAsync), OperationType.Delete);

        if (userId.IsNotValidMediaVaultId(errorContext with { FieldName = nameof(userId) }, out var userIdError))
        {
            ServiceValidationLogging.LogValidationFailure(
                _logger,
                [userIdError],
                GetType().Name,
                nameof(DeleteOwnAccountAsync),
                errorContext);
            return Result.ValidationFailure(
                [userIdError],
                MediaVaultResultMessages.ValidationFailure);
        }

        return await _userRepo.DeleteAccountAsync(userId, ct);
    }

    private static ErrorContext DefineErrorContext(string methodName, OperationType operation) =>
        new(operation: operation, entityName: nameof(UserEntity));

    private static string ProfileAvailabilityMessage(bool usernameUnavailable, bool emailUnavailable) =>
        (usernameUnavailable, emailUnavailable) switch
        {
            (true, true) => "The username and email are already registered.",
            (true, false) => "The username is already taken.",
            (false, true) => "The email is already registered.",
            _ => "User profile update validation failed."
        };
}
