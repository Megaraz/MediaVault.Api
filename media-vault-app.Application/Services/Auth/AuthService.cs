using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Mappers.User;
using media_vault_app.Application.Validators.User;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services.Auth
{
    public class AuthService : IAuthService
    {

        private readonly IUserRepo _userRepo;
        private readonly IPasswordHasherService _passwordHasherService;
        private readonly UserDtoValidator _dtoValidator;
        private readonly UserEntityMapper _entityToDtoMapper = new();
        private readonly UserDtoMapper _dtoToEntityMapper = new();
        public AuthService(IUserRepo userRepo, IPasswordHasherService passwordHasherService)
        {
            _userRepo = userRepo;
            _passwordHasherService = passwordHasherService;
            _dtoValidator = new UserDtoValidator();
        }

        public async Task<Result<UserDetailedDto>> LoginAsync(UserLoginDto loginDto, CancellationToken ct = default)
        {
            var errorContext = DefineErrorContext(nameof(LoginAsync), OperationType.Login);

            if (!_dtoValidator.IsValidLoginDto(loginDto, errorContext, out var validationErrors))
            {
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
                var invalidPasswordErrorContext = DefineErrorContext(nameof(LoginAsync), OperationType.Login, nameof(UserLoginDto.Password));
                invalidPasswordErrorContext.FieldName = "Password";
                var unauthorizedError = Error.Unauthorized(invalidPasswordErrorContext);

                return Result<UserDetailedDto>.Failure(unauthorizedError, "Invalid username/email or password.");
            }

            return repoResult.Map(_entityToDtoMapper.ToDetailedDTO);
        }

        public async Task<Result> RegisterUserAsync(UserRegisterDto registerDto, CancellationToken ct = default)
        {
            var dtoValidationErrorContext = DefineErrorContext(nameof(RegisterUserAsync), OperationType.Create);

            if (!_dtoValidator.IsValidRegisterDto(registerDto, dtoValidationErrorContext, out var dtoValidationErrors))
            {
                return Result.ValidationFailure(dtoValidationErrors, "User register validation failed.");
            }

            var availabilityResult = await _userRepo.CheckRegistrationAvailabilityAsync(registerDto.Username, registerDto.Email, ct);

            if (availabilityResult.IsFailure)
            {
                if (availabilityResult.PrimaryError.Type != ErrorType.Validation)
                    return availabilityResult;

                return Result.ValidationFailure(availabilityResult.ValidationErrors, "User register validation failed.");
            }

            var registrationValidationErrors = new List<ValidationError>();

            if (!availabilityResult.Value.IsUserNameAvailable)
            {
                var userNameErrorContext = DefineErrorContext(nameof(RegisterUserAsync), OperationType.Create, nameof(UserRegisterDto.Username));
                registrationValidationErrors.Add(ValidationError.AlreadyExists(userNameErrorContext));
            }

            if (!availabilityResult.Value.IsEmailAvailable)
            {
                var emailErrorContext = DefineErrorContext(nameof(RegisterUserAsync), OperationType.Create, nameof(UserRegisterDto.Email));
                registrationValidationErrors.Add(ValidationError.AlreadyExists(emailErrorContext));
            }

            if (registrationValidationErrors.Count > 0)
            {
                return Result.ValidationFailure(registrationValidationErrors, "User register validation failed.");
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

        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null, string? confirmFieldName = null)
        {
            return new ErrorContext(
                layer: "Service",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: "User",
                fieldName: fieldName,
                confirmFieldName: confirmFieldName);
        }
    }
}
