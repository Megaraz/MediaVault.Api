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
using Rasmus.SharedKernel.Interfaces.Validators;
using media_vault_app.Application.Mappers.User;

namespace media_vault_app.Application.Services.User
{
    public class UserWriteService
        : WriteServiceBase<UserEntitiy, Guid, UserCreateDto, UserDetailedDto>, IUserWriteService
    {
        private readonly IUserRepo _userRepo;
        private readonly IPasswordHasherService _passwordHasherService;
        private readonly UserDtoValidator _dtoValidator;
        private readonly UserEntityMapper _entityToDtoMapper;
        private readonly UserDtoMapper _dtoToEntityMapper;

        public UserWriteService(
            IUserRepo userRepo,
            IMapEntityToDetailedDto<UserEntitiy, UserDetailedDto> entityToDtoMapper,
            IPasswordHasherService passwordHasherService)
            : this(
                userRepo,
                new UserEntityMapper(),
                new UserDtoMapper(),
                passwordHasherService,
                new UserDtoValidator())
        {
        }

        private UserWriteService(
            IUserRepo userRepo,
            UserEntityMapper entityToDtoMapper,
            UserDtoMapper dtoToEntityMapper,
            IPasswordHasherService passwordHasherService,
            UserDtoValidator dtoValidator)
            : base(userRepo, entityToDtoMapper, dtoToEntityMapper, dtoValidator)
        {
            _entityToDtoMapper = entityToDtoMapper;
            _dtoToEntityMapper = dtoToEntityMapper;
            _userRepo = userRepo;
            _passwordHasherService = passwordHasherService;
            _dtoValidator = dtoValidator;
        }

        public override async Task<Result<UserDetailedDto>> CreateAsync(UserCreateDto createDto, CancellationToken ct = default)
        {
            if (createDto is null)
            {
                return await base.CreateAsync(createDto!, ct);
            }

            var errorContext = DefineErrorContext(nameof(CreateAsync), OperationType.Create, typeof(UserCreateDto).Name);

            if (!_dtoValidator.IsValidCreateDto(createDto, errorContext, out var validationErrors))
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
            var errorContext = DefineErrorContext(nameof(LoginAsync), OperationType.Login, typeof(UserLoginDto).Name);

            //if (!_userDtoValidator.IsValidLoginDto(loginDto, errorContext, out var validationErrors))
            if (!_dtoValidator.IsValidLoginDto(loginDto, errorContext, out var validationErrors))
            {
                return Result<UserDetailedDto>.ValidationFailure(validationErrors, "User login validation failed.");
            }

            var repoResult = await _userRepo.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail, ct);

            if (repoResult.IsFailure)
            {
                return Result<UserDetailedDto>.Failure(repoResult.PrimaryError, repoResult.Message);
            }

            bool passwordIsValid = _passwordHasherService.VerifyPassword(repoResult.Value.PasswordHash, loginDto.Password);

            // TODO: "The unauthorized error created here uses a hard-coded code string ("Unauthorized") instead of the structured ErrorCode.For(...) pattern used elsewhere. This makes error codes inconsistent and harder to handle uniformly (especially with ResultResponseMapper returning codes to clients). Consider adding/using an Error.Unauthorized(...) factory that uses OperationType.Login + ErrorReasonCode.GeneralUnauthorized / UserLoginFailure."
            if (!passwordIsValid)
            {
                var unauthorizedError = new Error(
                    "Unauthorized",
                    $"{errorContext.DescriptionPrefix}: Invalid username/email or password.",
                    ErrorType.Unauthorized);

                return Result<UserDetailedDto>.Failure(unauthorizedError, "Invalid username/email or password.");
            }

            return repoResult.Map(_entityToDtoMapper.ToDetailedDTO);
        }

        // TODO: Implement UpdatePasswordAsync method, which should validate the new password, hash it, and update the user's password in the repository.
        //public async Task<Result> UpdatePasswordAsync(Guid id, UserUpdateDto updateDto, CancellationToken ct = default)
        //{

        //}

        public async Task<Result> UpdateUserInfoAsync(Guid id, UserUpdateDto updateDto, CancellationToken ct = default)
        {
            var errorContext = DefineErrorContext(nameof(UpdateUserInfoAsync), OperationType.Update);

            List<ValidationError> validationErrors = new();

            if (!id.IsValidId(errorContext, out var idError))
                validationErrors.Add(idError);

            if (!_dtoValidator.IsValidUpdateDto(updateDto, errorContext, out var updateValidationErrors))
                validationErrors.AddRange(updateValidationErrors);

            if (validationErrors.Count > 0)
            {
                return Result.ValidationFailure(validationErrors, "User update validation failed.");
            }

            var userEntity = _dtoToEntityMapper.MapToEntity(id, updateDto);

            return (await _userRepo.UpdateAsync(userEntity, ct));
        }

        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? entityName = null)
        {
            return new ErrorContext(
                layer: "Service",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: typeof(UserEntitiy).Name);
        }
    }
}
