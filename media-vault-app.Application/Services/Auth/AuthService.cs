using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Mappers.User;
using media_vault_app.Application.Services.User;
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
            var errorContext = DefineErrorContext(nameof(LoginAsync), OperationType.Login, typeof(UserLoginDto).Name);

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
                var unauthorizedError = Error.Unauthorized(errorContext);

                return Result<UserDetailedDto>.Failure(unauthorizedError, "Invalid username/email or password.");
            }

            return repoResult.Map(_entityToDtoMapper.ToDetailedDTO);
        }

        public async Task<Result> RegisterUserAsync(UserRegisterDto registerDto, CancellationToken ct = default)
        {
            var errorContext = DefineErrorContext(nameof(RegisterUserAsync), OperationType.Create, typeof(UserRegisterDto).Name);

            if (!_dtoValidator.IsValidCreateDto(registerDto, errorContext, out var validationErrors))
            {
                return Result<UserDetailedDto>.ValidationFailure(validationErrors, "User register validation failed.");
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

        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? entityName = null)
        {
            return new ErrorContext(
                layer: "Service",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: "User");
        }
    }
}
