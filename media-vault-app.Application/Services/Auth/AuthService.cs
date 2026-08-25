using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Identity;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Interfaces.Validators;
using media_vault_app.Application.Mappers.User;
using Microsoft.Extensions.Logging;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using media_vault_app.Application.Validation;

namespace media_vault_app.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepo _userRepo;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IUserDtoValidator _dtoValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepo userRepo,
        IPasswordHasherService passwordHasherService,
        IUserDtoValidator dtoValidator,
        ILogger<AuthService> logger)
    {
        _userRepo = userRepo;
        _passwordHasherService = passwordHasherService;
        _dtoValidator = dtoValidator;
        _logger = logger;
    }

    public async Task<Result<UserDetailedDto>> LoginAsync(
        UserLoginDto loginDto,
        CancellationToken ct = default)
    {
        var baseErrorContext = DefineErrorContext(nameof(LoginAsync), OperationType.Login);
        var canonicalLoginDto = loginDto is null
            ? null
            : UserIdentifierCanonicalizer.Canonicalize(loginDto);

        if (!_dtoValidator.IsValidLoginDto(canonicalLoginDto!, baseErrorContext, out var validationErrors))
        {
            ServiceValidationLogging.LogValidationFailure(
                _logger, validationErrors, nameof(AuthService), nameof(LoginAsync), baseErrorContext);
            return Result<UserDetailedDto>.ValidationFailure(
                validationErrors,
                "Invalid username/email or password.");
        }

        var repoResult = await _userRepo.GetByUsernameOrEmailAsync(
            canonicalLoginDto!.UsernameOrEmail,
            ct);

        if (repoResult.IsFailure)
            return Result<UserDetailedDto>.Failure(repoResult.PrimaryError, repoResult.Message);

        var passwordIsValid = _passwordHasherService.VerifyPassword(
            repoResult.Value.PasswordHash,
            canonicalLoginDto.Password);

        if (!passwordIsValid)
        {
            var invalidPasswordErrorContext = baseErrorContext with
            {
                FieldName = nameof(UserLoginDto.Password)
            };
            var unauthorizedError = MediaVaultErrors.Unauthorized(invalidPasswordErrorContext);

            return Result<UserDetailedDto>.Failure(
                unauthorizedError,
                "Invalid username/email or password.");
        }

        return repoResult.Map(UserAccountMapper.ToDetailedDto);
    }

    public async Task<Result> RegisterUserAsync(
        UserRegisterDto registerDto,
        CancellationToken ct = default)
    {
        var baseErrorContext = DefineErrorContext(nameof(RegisterUserAsync), OperationType.Create);
        var canonicalRegisterDto = registerDto is null
            ? null
            : UserIdentifierCanonicalizer.Canonicalize(registerDto);

        if (!_dtoValidator.IsValidCreateDto(canonicalRegisterDto!, baseErrorContext, out var dtoValidationErrors))
        {
            ServiceValidationLogging.LogValidationFailure(
                _logger, dtoValidationErrors, nameof(AuthService), nameof(RegisterUserAsync), baseErrorContext);
            return Result.ValidationFailure(
                dtoValidationErrors,
                "User register validation failed.");
        }

        var availabilityResult = await _userRepo.CheckRegistrationAvailabilityAsync(
            canonicalRegisterDto!.Username,
            canonicalRegisterDto.Email,
            ct);

        if (availabilityResult.IsFailure)
        {
            if (availabilityResult.PrimaryError.Type != ErrorType.Validation)
                return availabilityResult;

            ServiceValidationLogging.LogValidationFailure(
                _logger,
                availabilityResult.ValidationErrors,
                nameof(AuthService),
                nameof(RegisterUserAsync),
                baseErrorContext);
            return Result.ValidationFailure(
                availabilityResult.ValidationErrors,
                "User register validation failed.");
        }

        var registrationValidationErrors = new List<ValidationError>();

        if (!availabilityResult.Value.IsUserNameAvailable)
        {
            var userNameErrorContext = baseErrorContext with
            {
                FieldName = nameof(UserRegisterDto.Username)
            };
            registrationValidationErrors.Add(MediaVaultValidationError.AlreadyExists(userNameErrorContext));
        }

        if (!availabilityResult.Value.IsEmailAvailable)
        {
            var emailErrorContext = baseErrorContext with
            {
                FieldName = nameof(UserRegisterDto.Email)
            };
            registrationValidationErrors.Add(MediaVaultValidationError.AlreadyExists(emailErrorContext));
        }

        if (registrationValidationErrors.Count > 0)
        {
            ServiceValidationLogging.LogValidationFailure(
                _logger,
                registrationValidationErrors,
                nameof(AuthService),
                nameof(RegisterUserAsync),
                baseErrorContext);

            return Result.ValidationFailure(
                registrationValidationErrors,
                RegistrationAvailabilityMessage(
                    !availabilityResult.Value.IsUserNameAvailable,
                    !availabilityResult.Value.IsEmailAvailable));
        }

        var hashedPassword = _passwordHasherService.HashPassword(canonicalRegisterDto.Password);
        var hashedRegisterDto = canonicalRegisterDto with
        {
            Password = hashedPassword,
            ConfirmPassword = hashedPassword
        };
        var userEntity = UserAccountMapper.ToRegistrationEntity(hashedRegisterDto);

        return await _userRepo.RegisterUserAsync(userEntity, ct);
    }

    private static ErrorContext DefineErrorContext(
        string methodName,
        OperationType operation,
        string? fieldName = null) =>
        new(operation: operation, entityName: "User", fieldName: fieldName);

    private static string RegistrationAvailabilityMessage(bool usernameUnavailable, bool emailUnavailable) =>
        (usernameUnavailable, emailUnavailable) switch
        {
            (true, true) => "The username and email are already registered.",
            (true, false) => "The username is already taken.",
            (false, true) => "The email is already registered.",
            _ => "User register validation failed."
        };
}
