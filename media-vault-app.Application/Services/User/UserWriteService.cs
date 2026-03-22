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

namespace media_vault_app.Application.Services.User
{
    public class UserWriteService
        : WriteServiceBase<UserEntitiy, Guid, UserCreateDto, UserUpdateDto, UserDetailedDto>, IUserWriteService
    {
        private readonly IUserRepo _userRepo;
        private readonly IPasswordHasherService _passwordHasherService;
        private readonly UserDtoValidator _dtoValidator;
        private readonly IMapEntityToDetailedDto<UserEntitiy, UserDetailedDto> _entityToDtoMapper;

        public UserWriteService(
            IUserRepo userRepo,
            IMapEntityToDetailedDto<UserEntitiy, UserDetailedDto> entityToDtoMapper,
            IMapDtoToEntity<UserEntitiy, UserDetailedDto, UserCreateDto, Guid, UserUpdateDto> dtoToEntityMapper,
            IPasswordHasherService passwordHasherService)
            : this(
                userRepo,
                entityToDtoMapper,
                dtoToEntityMapper,
                passwordHasherService,
                new UserDtoValidator())
        {
        }

        private UserWriteService(
            IUserRepo userRepo,
            IMapEntityToDetailedDto<UserEntitiy, UserDetailedDto> entityToDtoMapper,
            IMapDtoToEntity<UserEntitiy, UserDetailedDto, UserCreateDto, Guid, UserUpdateDto> dtoToEntityMapper,
            IPasswordHasherService passwordHasherService,
            UserDtoValidator dtoValidator)
            : base(userRepo, entityToDtoMapper, dtoToEntityMapper, dtoValidator)
        {
            _entityToDtoMapper = entityToDtoMapper;
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

            var errorContext = CreateErrorContext(nameof(CreateAsync), OperationType.Create, typeof(UserCreateDto).Name);

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
            var errorContext = CreateErrorContext(nameof(LoginAsync), OperationType.Login, typeof(UserLoginDto).Name);

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

        private ErrorContext CreateErrorContext(string methodName, OperationType operation, string? entityName = null)
        {
            return new ErrorContext(
                layer: "Service",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: entityName ?? typeof(UserEntitiy).Name);
        }
    }
}
