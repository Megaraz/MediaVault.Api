using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Application.Mappers.User;
using Microsoft.Extensions.Logging;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using media_vault_app.Application.Results;
using media_vault_app.Application.Validation;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Services.User;

public class UserReadService : IUserReadService
{
    private readonly IUserRepo _userRepo;
    private readonly ILogger<UserReadService> _logger;

    public UserReadService(
        IUserRepo userRepo,
        ILogger<UserReadService> logger)
    {
        _userRepo = userRepo;
        _logger = logger;
    }

    public async Task<Result<UserDetailedDto>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var errorContext = DefineErrorContext(nameof(GetCurrentUserAsync), OperationType.Get);

        if (userId.IsNotValidMediaVaultId(errorContext with { FieldName = nameof(userId) }, out var userIdError))
        {
            ServiceValidationLogging.LogValidationFailure(
                _logger,
                [userIdError],
                GetType().Name,
                nameof(GetCurrentUserAsync),
                errorContext);
            return Result<UserDetailedDto>.ValidationFailure(
                [userIdError],
                MediaVaultResultMessages.ValidationFailure);
        }

        var repoResult = await _userRepo.GetByIdAsync(userId, ct);
        return repoResult.Map(UserAccountMapper.ToDetailedDto);
    }

    private static ErrorContext DefineErrorContext(string methodName, OperationType operation) =>
        new(operation: operation, entityName: nameof(UserEntity));
}
