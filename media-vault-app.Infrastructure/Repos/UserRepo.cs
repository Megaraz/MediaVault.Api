using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;
using Microsoft.Data.SqlClient;
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
            catch (DbUpdateException dbEx)
            {
                if (dbEx.InnerException is SqlException sqlEx &&
                    (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                {
                    return Result.Failure(Error.Conflict(errorContext), "A conflict occurred while creating the entity.");
                }
                else
                {
                    errorContext.DescriptionSuffix = $"An error occurred while creating the {errorContext.EntityName}.";
                    return Result.Failure(Error.DbCreateFailure(errorContext, dbEx), errorContext.DescriptionSuffix);
                }
            }

        }

        public async Task<Result<bool>> IsUserNameAvailable(string username, CancellationToken ct = default)
        {
            var errorContext = DefineErrorContext(nameof(IsUserNameAvailable), OperationType.Get, "Username");
            if (username.IsNullOrWhiteSpace(errorContext, out var requiredValueError))
                return Result<bool>.ValidationFailure([requiredValueError], errorContext.DescriptionSuffix!);
            try
            {
                string lookupUsername = username.Trim();
                bool usernameExists = await _dbSet
                    .AsNoTracking()
                    .AnyAsync(currentUser => currentUser.Username == lookupUsername, ct);

                return Result<bool>.Success(!usernameExists);
            }
            catch (Exception ex)
            {
                errorContext.DescriptionSuffix = "An error occurred while checking the username.";
                return Result<bool>.Failure(
                    Error.DbGetFailure(errorContext, ex),
                    errorContext.DescriptionSuffix);
            }
        }
        public async Task<Result<bool>> IsEmailAvailable(string email, CancellationToken ct = default)
        {
            var errorContext = DefineErrorContext(nameof(IsEmailAvailable), OperationType.Get, "Email");
            if (email.IsNullOrWhiteSpace(errorContext, out var requiredValueError))
                return Result<bool>.ValidationFailure([requiredValueError], errorContext.DescriptionSuffix!);
            try
            {
                string lookupEmail = email.Trim();
                bool emailExists = await _dbSet
                    .AsNoTracking()
                    .AnyAsync(currentUser => currentUser.Email == lookupEmail, ct);

                return Result<bool>.Success(!emailExists);
            }
            catch (Exception ex)
            {
                errorContext.DescriptionSuffix = "An error occurred while checking the email.";
                return Result<bool>.Failure(
                    Error.DbGetFailure(errorContext, ex),
                    errorContext.DescriptionSuffix);
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
                        Error.Unauthorized(errorContext),
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

        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null, string? confirmFieldName = null)
        {
            return new ErrorContext(
                layer: "Infrastructure",
                serviceName: GetType().Name,
                methodName: methodName,
                operation: operation,
                entityName: typeof(User).Name,
                fieldName: fieldName,
                confirmFieldName: confirmFieldName);
        }
    }
}
