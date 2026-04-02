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
using media_vault_app.Application.Validators.User;

namespace media_vault_app.Application.Services.User
{
    public class UserWriteService
        : WriteServiceBase<UserEntitiy, Guid, UserRegisterDto, UserDetailedDto>, IUserWriteService
    {
        private readonly IUserRepo _userRepo;
        private readonly UserDtoValidator _dtoValidator;
        private readonly UserEntityMapper _entityToDtoMapper;
        private readonly UserDtoMapper _dtoToEntityMapper;

        public UserWriteService(
            IUserRepo userRepo
            )
            : this(
                userRepo,
                new UserEntityMapper(),
                new UserDtoMapper(),
                new UserDtoValidator())
        {
        }

        private UserWriteService(
            IUserRepo userRepo,
            UserEntityMapper entityToDtoMapper,
            UserDtoMapper dtoToEntityMapper,
            UserDtoValidator dtoValidator)
            : base(userRepo, entityToDtoMapper, dtoToEntityMapper, dtoValidator)
        {
            _entityToDtoMapper = entityToDtoMapper;
            _dtoToEntityMapper = dtoToEntityMapper;
            _userRepo = userRepo;
            _dtoValidator = dtoValidator;
        }

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
