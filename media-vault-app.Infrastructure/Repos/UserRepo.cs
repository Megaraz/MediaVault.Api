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
            string methodName = nameof(GetByUsernameOrEmailAsync);
            string errorDescriptionPrefix = $"An error occurred when trying to get the user by username or email in Infrastructure layer: {this.GetType().Name}.{methodName}()";

            if (string.IsNullOrWhiteSpace(usernameOrEmail))
            {
                string errorMessageReason = "A username or email is required and cannot be null or empty.";

                ValidationError requiredValueError = ValidationError.Required<User>(
                    OperationType.Get,
                    errorDescriptionPrefix,
                    nameof(usernameOrEmail),
                    errorMessageReason);

                return Result<User>.ValidationFailure([requiredValueError], errorMessageReason);
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
                        Error.NotFound<User>(errorDescriptionPrefix),
                        "Invalid username/email or password.");
                }

                return Result<User>.Success(user);
            }
            catch (Exception ex)
            {
                return Result<User>.Failure(
                    Error.DbGetFailure<User>(errorDescriptionPrefix, ex),
                    "An error occurred while retrieving the User.");
            }
        }
    }
}
