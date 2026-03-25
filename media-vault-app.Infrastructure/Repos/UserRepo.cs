using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.Repos
{
    public class UserRepo : GenericRepoEFCore<User, Guid>, IUserRepo
    {
        public UserRepo(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<Result> RegisterUserAsync(User entity, CancellationToken ct = default)
        {
            // Define error handling context
            var errorContext = DefineErrorContext(nameof(RegisterUserAsync), OperationType.Create);

            if (entity.IsNull(errorContext, out var nullValueError))
                return Result.ValidationFailure([nullValueError], errorContext.DescriptionSuffix!);

            try
            {
                _dbSet.Add(entity);
                await _appDbContext.SaveChangesAsync(ct);
                return Result.Success();
            }
            catch (Exception ex)
            {
                errorContext.DescriptionSuffix = $"An error occurred while creating the {errorContext.EntityName}.";

                Error dbCreateFailure = Error.DbCreateFailure(errorContext, ex);

                return Result.Failure(dbCreateFailure, "An error occurred while creating the entity.");
            }

        }

        public async Task<Result<User>> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default)
        {
            var errorContext = DefineErrorContext(nameof(GetByUsernameOrEmailAsync), OperationType.Get);

            if (usernameOrEmail.IsNullOrWhiteSpace(errorContext, out var requiredValueError))
                return Result<User>.ValidationFailure([requiredValueError], errorContext.DescriptionSuffix!);

            try
            {
                string lookupValue = usernameOrEmail.Trim();

                var user = await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        currentUser => currentUser.Username == lookupValue || currentUser.Email == lookupValue,
                        ct);

                if (user is null)
                {
                    return Result<User>.Failure(
                        Error.NotFound<User>(errorContext.DescriptionPrefix),
                        "Invalid username/email or password.");
                }

                return Result<User>.Success(user);
            }
            catch (Exception ex)
            {
                errorContext.DescriptionSuffix = "An error occurred while retrieving the User.";

                return Result<User>.Failure(
                    Error.DbGetFailure(errorContext, ex),
                    errorContext.DescriptionSuffix);
            }
        }

        private ErrorContext DefineErrorContext(string methodName, OperationType operation)
        {
            return new ErrorContext(
                layer: "Infrastructure",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: typeof(User).Name);
        }
    }
}
