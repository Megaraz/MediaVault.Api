using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using UserEntitiy = media_vault_app.Domain.Entities.User;
using Rasmus.SharedKernel.ResultPattern;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces;

namespace media_vault_app.Application.Services.User
{
    public class UserWriteService : WriteServiceBase<UserEntitiy, Guid, UserCreateDto, UserUpdateDto, UserDetailedDto>, IUserWriteService
    {
        private readonly IUserRepo _userRepo;
        private readonly IPasswordHasherService _passwordHasherService;

        public UserWriteService(
                IUserRepo userRepo,
                IMapEntityToDetailedDto<UserEntitiy, UserDetailedDto> entityToDtoMapper,
                IMapDtoToEntity<UserEntitiy, UserDetailedDto, UserCreateDto, Guid, UserUpdateDto> dtoToEntityMapper,
                IPasswordHasherService passwordHasherService
            ) : base(userRepo, entityToDtoMapper, dtoToEntityMapper)
        {
            _userRepo = userRepo;
            _passwordHasherService = passwordHasherService;
        }

        public override async Task<Result<UserDetailedDto>> CreateAsync(UserCreateDto createDto, CancellationToken ct = default)
        {
            if (createDto is null)
            {
                return await base.CreateAsync(createDto!, ct);
            }

            List<ValidationError> validationErrors = [];

            string methodName = nameof(CreateAsync);
            string errorDescriptionPrefix = $"An error occurred when trying to create the entity in Service Layer: {this.GetType().Name}.{methodName}()";

            if (!string.Equals(createDto.Email, createDto.ConfirmEmail, StringComparison.OrdinalIgnoreCase))
            {
                validationErrors.Add(
                    ValidationError.Custom<UserCreateDto>(
                        OperationType.Create,
                        errorDescriptionPrefix,
                        "Email and ConfirmEmail must match."));
            }

            if (!string.Equals(createDto.Password, createDto.ConfirmPassword, StringComparison.Ordinal))
            {
                validationErrors.Add(
                    ValidationError.Custom<UserCreateDto>(
                        OperationType.Create,
                        errorDescriptionPrefix,
                        "Password and ConfirmPassword must match."));
            }

            if (validationErrors.Count > 0)
            {
                return Result<UserDetailedDto>.ValidationFailure(validationErrors, "User creation validation failed.");
            }

            string hashedPassword = _passwordHasherService.HashPassword(createDto.Password);

            var hashedCreateDto = createDto with
            {
                Password = hashedPassword,
                ConfirmPassword = hashedPassword
            };

            return await base.CreateAsync(hashedCreateDto, ct);
        }

        public async Task<Result<UserDetailedDto>> LoginAsync(UserLoginDto loginDto, CancellationToken ct = default)
        {
            string methodName = nameof(LoginAsync);
            string errorDescriptionPrefix = $"An error occurred when trying to log in the user in Service Layer: {this.GetType().Name}.{methodName}()";

            if (loginDto is null)
            {
                string errorMessageReason = "Login data is required and cannot be null.";

                ValidationError nullValueError = ValidationError.Required<UserLoginDto>(
                    OperationType.Get,
                    errorDescriptionPrefix,
                    nameof(loginDto),
                    errorMessageReason);

                return Result<UserDetailedDto>.ValidationFailure([nullValueError], errorMessageReason);
            }

            var userResult = await _userRepo.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail, ct);

            if (userResult.IsFailure)
            {
                return Result<UserDetailedDto>.Failure(userResult.PrimaryError, userResult.Message);
            }

            bool passwordIsValid = _passwordHasherService.VerifyPassword(userResult.Value.PasswordHash, loginDto.Password);

            if (!passwordIsValid)
            {
                var unauthorizedError = new Error(
                    "USER_LOGIN_INVALID_CREDENTIALS",
                    $"{errorDescriptionPrefix}: Invalid username/email or password.",
                    ErrorType.Unauthorized);

                return Result<UserDetailedDto>.Failure(unauthorizedError, "Invalid username/email or password.");
            }

            var user = userResult.Value;

            return Result<UserDetailedDto>.Success(
                new UserDetailedDto(user.Id, user.Username, user.Email, user.CreatedAtUtc));
        }
    }
}
