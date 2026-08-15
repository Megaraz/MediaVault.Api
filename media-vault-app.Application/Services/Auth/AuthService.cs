using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Mappers;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Interfaces.Validators;
using Microsoft.Extensions.Logging;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using Rasmus.SharedKernel.Validation;

namespace media_vault_app.Application.Services.Auth
{
    public class AuthService : IAuthService
    {

        private readonly IUserRepo _userRepo;
        private readonly IPasswordHasherService _passwordHasherService;
        private readonly IUserDtoValidator _dtoValidator;
        private readonly IUserEntityMapper _entityToDtoMapper;
        private readonly IUserDtoMapper _dtoToEntityMapper;
        private readonly ILogger<AuthService> _logger;
        public AuthService(
            IUserRepo userRepo,
            IPasswordHasherService passwordHasherService,
            IUserEntityMapper entityToDtoMapper,
            IUserDtoMapper dtoToEntityMapper,
            IUserDtoValidator dtoValidator,
            ILogger<AuthService> logger
            )
        {
            _userRepo = userRepo;
            _passwordHasherService = passwordHasherService;
            _entityToDtoMapper = entityToDtoMapper;
            _dtoToEntityMapper = dtoToEntityMapper;
            _dtoValidator = dtoValidator;
            _logger = logger;
        }

        public async Task<Result<UserDetailedDto>> LoginAsync(UserLoginDto loginDto, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(LoginAsync), OperationType.Login);

            if (!_dtoValidator.IsValidLoginDto(loginDto, baseErrorContext, out var validationErrors))
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, validationErrors, nameof(AuthService), nameof(LoginAsync), baseErrorContext);
                return Result<UserDetailedDto>.ValidationFailure(validationErrors, "Invalid username/email or password.");
            }

            var repoResult = await _userRepo.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail, ct);

            if (repoResult.IsFailure)
            {
                return Result<UserDetailedDto>.Failure(repoResult.PrimaryError, repoResult.Message);
            }

            bool passwordIsValid = _passwordHasherService.VerifyPassword(repoResult.Value.PasswordHash, loginDto.Password);

            if (!passwordIsValid)
            {
                var invalidPasswordErrorContext = baseErrorContext with { FieldName = nameof(UserLoginDto.Password) };
                var unauthorizedError = MediaVaultErrors.Unauthorized(invalidPasswordErrorContext);

                return Result<UserDetailedDto>.Failure(unauthorizedError, "Invalid username/email or password.");
            }

            return repoResult.Map(_entityToDtoMapper.ToDetailedDto);
        }

        public async Task<Result> RegisterUserAsync(UserRegisterDto registerDto, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(RegisterUserAsync), OperationType.Create);

            if (!_dtoValidator.IsValidCreateDto(registerDto, baseErrorContext, out var dtoValidationErrors))
            {
                ServiceValidationLogging.LogValidationFailure(
                    _logger, dtoValidationErrors, nameof(AuthService), nameof(RegisterUserAsync), baseErrorContext);
                return Result.ValidationFailure(dtoValidationErrors, "User register validation failed.");
            }

            var availabilityResult = await _userRepo.CheckRegistrationAvailabilityAsync(registerDto.Username, registerDto.Email, ct);

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
                return Result.ValidationFailure(availabilityResult.ValidationErrors, "User register validation failed.");
            }

            var registrationValidationErrors = new List<ValidationError>();

            if (!availabilityResult.Value.IsUserNameAvailable)
            {
                var userNameErrorContext = baseErrorContext with { FieldName = nameof(UserRegisterDto.Username) };
                registrationValidationErrors.Add(MediaVaultValidationError.AlreadyExists(userNameErrorContext));
            }

            if (!availabilityResult.Value.IsEmailAvailable)
            {
                var emailErrorContext = baseErrorContext with { FieldName = nameof(UserRegisterDto.Email) };
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

            string hashedPassword = _passwordHasherService.HashPassword(registerDto.Password);

            var hashedCreateDto = registerDto with
            {
                Password = hashedPassword,
                ConfirmPassword = hashedPassword
            };

            var userEntity = _dtoToEntityMapper.ToEntity(hashedCreateDto);

            return await _userRepo.RegisterUserAsync(userEntity, ct);
        }

        // TODO: Implement UpdatePasswordAsync method, which should validate the new password, hash it, and update the user's password in the repository.
        //public async Task<Result> UpdatePasswordAsync(Guid id, UserUpdateDto updateDto, CancellationToken ct = default)
        //{

        //}

        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null)
        {
            return new ErrorContext(
                operation: operation,
                entityName: "User",
                fieldName: fieldName);
        }

        private static string RegistrationAvailabilityMessage(bool usernameUnavailable, bool emailUnavailable) =>
            (usernameUnavailable, emailUnavailable) switch
            {
                (true, true) => "The username and email are already registered.",
                (true, false) => "The username is already taken.",
                (false, true) => "The email is already registered.",
                _ => "User register validation failed."
            };
    }
}
