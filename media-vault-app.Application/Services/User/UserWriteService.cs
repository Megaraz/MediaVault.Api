using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Identity;
using media_vault_app.Application.Interfaces.Mappers;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Interfaces.Validators;
using media_vault_app.Application.Services.Base_Classes;
using Microsoft.Extensions.Logging;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Results;
using Rasmus.SharedKernel.Validation;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Services.User
{
    public class UserWriteService
        : WriteServiceBase<UserEntity, Guid, UserRegisterDto, UserUpdateDto, UserDetailedDto>, IUserWriteService
    {
        private readonly IUserRepo _userRepo;

        public UserWriteService(
            IUserRepo repo,
            IUserEntityMapper entityMapper,
            IUserDtoMapper dtoMapper,
            IUserDtoValidator validator,
            ILogger<UserWriteService> logger
            ) : base(repo, entityMapper, dtoMapper, validator, logger)
        {
            _userRepo = repo;
        }

        public override Task<Result<UserDetailedDto>> CreateAsync(
            UserRegisterDto createDto,
            CancellationToken ct)
        {
            var canonicalCreateDto = createDto is null
                ? null
                : UserIdentifierCanonicalizer.Canonicalize(createDto);

            return base.CreateAsync(canonicalCreateDto!, ct);
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

            return await _userRepo.UpdateProfileAsync(userId, username, email, ct);
        }

        private static string ProfileAvailabilityMessage(bool usernameUnavailable, bool emailUnavailable) =>
            (usernameUnavailable, emailUnavailable) switch
            {
                (true, true) => "The username and email are already registered.",
                (true, false) => "The username is already taken.",
                (false, true) => "The email is already registered.",
                _ => "User profile update validation failed."
            };
    }
}
