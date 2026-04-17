using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Infrastructure.Repos
{
    public class UserRepo : GenericRepoBase<User, Guid>, IUserRepo
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

        public async Task<Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>> CheckRegistrationAvailabilityAsync(string username, string email, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(CheckRegistrationAvailabilityAsync), OperationType.Get);
            var validationErrors = new List<ValidationError>();

            var usernameErrorContext = baseErrorContext with { FieldName = "Username" };
            if (username.IsNullOrWhiteSpace(usernameErrorContext, out var usernameRequiredError))
                validationErrors.Add(usernameRequiredError);

            var emailErrorContext = baseErrorContext with { FieldName = "Email" };
            if (email.IsNullOrWhiteSpace(emailErrorContext, out var emailRequiredError))
                validationErrors.Add(emailRequiredError);

            if (validationErrors.Count > 0)
                return Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.ValidationFailure(validationErrors);

            try
            {
                string lookupUsername = username.Trim();
                string lookupEmail = email.Trim();

                var matchingUsers = await _dbSet
                    .AsNoTracking()
                    .Where(currentUser => currentUser.Username == lookupUsername || currentUser.Email == lookupEmail)
                    .Select(currentUser => new { currentUser.Username, currentUser.Email })
                    .ToListAsync(ct);

                bool usernameExists = matchingUsers.Any(currentUser => currentUser.Username == lookupUsername);
                bool emailExists = matchingUsers.Any(currentUser => currentUser.Email == lookupEmail);

                return Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.Success((!usernameExists, !emailExists));
            }
            catch (Exception ex)
            {
                baseErrorContext.DescriptionSuffix = "An error occurred while checking the username and email.";
                return Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.Failure(
                    Error.DbGetFailure(baseErrorContext, ex),
                    baseErrorContext.DescriptionSuffix);
            }
        }

        public async Task<Result<User>> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetByUsernameOrEmailAsync), OperationType.Get, "UsernameOrEmail");

            if (usernameOrEmail.IsNullOrWhiteSpace(baseErrorContext, out var requiredValueError))
                return Result<User>.ValidationFailure([requiredValueError]);

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
                        Error.Unauthorized(baseErrorContext),
                        "Invalid username/email or password.");
                }

                return Result<User>.Success(user);
            }
            catch (Exception ex)
            {
                var dbGeneralExceptionErrorContext = baseErrorContext with { DescriptionSuffix = "An error occurred while retrieving the User." };

                return Result<User>.Failure(
                    Error.DbGetFailure(dbGeneralExceptionErrorContext, ex),
                    dbGeneralExceptionErrorContext.DescriptionSuffix);
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
