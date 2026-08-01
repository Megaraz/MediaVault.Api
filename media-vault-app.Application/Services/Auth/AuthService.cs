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
                _logger.LogDebug("LoginAsync validation failed: {ValidationErrors}", ServiceValidationLogging.FormatValidationErrors(validationErrors));
                return Result<UserDetailedDto>.ValidationFailure(validationErrors, "Invalid username/email or password.");
            }

            var repoResult = await _userRepo.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail, ct);

            if (repoResult.IsFailure)
            {
                _logger.LogDebug("LoginAsync GetUser failed: {Code} — {Description}",
                repoResult.PrimaryError.Code, repoResult.PrimaryError.Description);

                return Result<UserDetailedDto>.Failure(repoResult.PrimaryError, repoResult.Message);
            }

            bool passwordIsValid = _passwordHasherService.VerifyPassword(repoResult.Value.PasswordHash, loginDto.Password);

            if (!passwordIsValid)
            {
                var invalidPasswordErrorContext = baseErrorContext with { FieldName = nameof(UserLoginDto.Password) };
                var unauthorizedError = MediaVaultErrors.Unauthorized(invalidPasswordErrorContext);

                return Result<UserDetailedDto>.Failure(unauthorizedError, "Invalid username/email or password.");
            }

            var mappedRepoResult = repoResult.Map(_entityToDtoMapper.ToDetailedDto);
            if (mappedRepoResult.IsFailure)
                _logger.LogDebug("LoginAsync mapping failed: {Code} — {Description}", mappedRepoResult.PrimaryError.Code, mappedRepoResult.PrimaryError.Description);
            return mappedRepoResult;
        }

        public async Task<Result> RegisterUserAsync(UserRegisterDto registerDto, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(RegisterUserAsync), OperationType.Create);

            if (!_dtoValidator.IsValidCreateDto(registerDto, baseErrorContext, out var dtoValidationErrors))
            {
                _logger.LogDebug("RegisterUserAsync validation failed: {ValidationErrors}", ServiceValidationLogging.FormatValidationErrors(dtoValidationErrors));
                return Result.ValidationFailure(dtoValidationErrors, "User register validation failed.");
            }

            var availabilityResult = await _userRepo.CheckRegistrationAvailabilityAsync(registerDto.Username, registerDto.Email, ct);

            if (availabilityResult.IsFailure)
            {

                _logger.LogDebug("RegisterUser failed: {Code} — {Description}",
                availabilityResult.PrimaryError.Code, availabilityResult.PrimaryError.Description);

                if (availabilityResult.PrimaryError.Type != ErrorType.Validation)
                    return availabilityResult;

                _logger.LogDebug("RegisterUserAsync availability validation failed: {ValidationErrors}", ServiceValidationLogging.FormatValidationErrors(availabilityResult.ValidationErrors));
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
                _logger.LogDebug("RegisterUserAsync registration validation failed: {ValidationErrors}",
                    ServiceValidationLogging.FormatValidationErrors(registrationValidationErrors));

                return Result.ValidationFailure(registrationValidationErrors, "User register validation failed.");
            }

            string hashedPassword = _passwordHasherService.HashPassword(registerDto.Password);

            var hashedCreateDto = registerDto with
            {
                Password = hashedPassword,
                ConfirmPassword = hashedPassword
            };

            var userEntity = _dtoToEntityMapper.ToEntity(hashedCreateDto);

            var mappedRepoResult = await _userRepo.RegisterUserAsync(userEntity, ct);

            if (mappedRepoResult.IsFailure)
            {
                _logger.LogDebug("RegisterUserAsync failed: {Code} — {Description}",
                    mappedRepoResult.PrimaryError.Code, mappedRepoResult.PrimaryError.Description);
            }

            return mappedRepoResult;
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
    }
}
