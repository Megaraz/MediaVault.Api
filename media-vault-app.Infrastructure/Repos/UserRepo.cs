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

        public async Task<Result<User>> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default)
        {
            var errorContext = DefineErrorContext(nameof(GetByUsernameOrEmailAsync), OperationType.Get);


            if (string.IsNullOrWhiteSpace(usernameOrEmail))
            {
                errorContext.DescriptionSuffix = "A username or email is required and cannot be null or empty.";

                ValidationError requiredValueError = ValidationError.Required(errorContext);

                return Result<User>.ValidationFailure([requiredValueError], errorContext.DescriptionSuffix);
            }

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
                    "An error occurred while retrieving the User.");
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
