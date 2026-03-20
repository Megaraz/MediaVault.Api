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

            var errorContext = new ErrorContext(
                layer: "Infrastructure",
                serviceName: this.GetType().Name,
                methodName: nameof(GetByUsernameOrEmailAsync),
                operation: OperationType.Get,
                entityName: typeof(User).Name
            );


            if (string.IsNullOrWhiteSpace(usernameOrEmail))
            {
                errorContext.DescriptionSuffix = "A username or email is required and cannot be null or empty.";

                ValidationError requiredValueError = ValidationError.Required<User>(errorContext);

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
                return Result<User>.Failure(
                    Error.DbGetFailure<User>(errorContext.DescriptionPrefix, ex),
                    "An error occurred while retrieving the User.");
            }
        }
    }
}
