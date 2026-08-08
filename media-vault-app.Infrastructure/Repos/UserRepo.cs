using System.Data.Common;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using media_vault_app.Infrastructure.Diagnostics;

namespace media_vault_app.Infrastructure.Repos
{
    public class UserRepo : RepoBase<User, Guid>, IUserRepo
    {
        public UserRepo(
            AppDbContext appDbContext,
            ErrorEventLogger<RepoBase<User, Guid>> errorEventLogger)
            : base(appDbContext, errorEventLogger)
        {
        }

        public async Task<Result> RegisterUserAsync(User entity, CancellationToken ct = default)
        {
            try
            {
                _dbSet.Add(entity);
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
                return Result.Success();
            }
            catch (DbUpdateException dbEx)
            {
                var baseErrorContext = DefineErrorContext(nameof(RegisterUserAsync), OperationType.Create);

                if (dbEx.InnerException is DbException dbInnerEx &&
                    (dbInnerEx.ErrorCode == 2601 || dbInnerEx.ErrorCode == 2627))
                {
                    return Result.Failure(MediaVaultErrors.Conflict(baseErrorContext));
                }

                return LogAndFail(
                    DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, dbEx),
                    baseErrorContext);
            }
            catch (OperationCanceledException)
            {
                var baseErrorContext = DefineErrorContext(nameof(RegisterUserAsync), OperationType.Create);
                return Result.Failure(MediaVaultErrors.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                var baseErrorContext = DefineErrorContext(nameof(RegisterUserAsync), OperationType.Create);
                return LogAndFail(
                    DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, ex),
                    baseErrorContext);
            }

        }

        public async Task<Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>> CheckRegistrationAvailabilityAsync(string username, string email, CancellationToken ct = default)
        {

            try
            {
                string lookupUsername = username.Trim();
                string lookupEmail = email.Trim();

                var matchingUsers = await _dbSet
                    .AsNoTracking()
                    .Where(currentUser => currentUser.Username == lookupUsername || currentUser.Email == lookupEmail)
                    .Select(currentUser => new { currentUser.Username, currentUser.Email })
                    .ToListAsync(ct).ConfigureAwait(false);

                bool usernameExists = matchingUsers.Any(currentUser => currentUser.Username == lookupUsername);
                bool emailExists = matchingUsers.Any(currentUser => currentUser.Email == lookupEmail);

                return Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.Success((!usernameExists, !emailExists));
            }
            catch (OperationCanceledException)
            {
                var baseErrorContext = DefineErrorContext(nameof(CheckRegistrationAvailabilityAsync), OperationType.Get);
                return Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.Failure(MediaVaultErrors.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                var baseErrorContext = DefineErrorContext(nameof(CheckRegistrationAvailabilityAsync), OperationType.Get);
                return LogAndFail<(bool IsUserNameAvailable, bool IsEmailAvailable)>(
                    DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex),
                    baseErrorContext);
            }
        }

        public async Task<Result<User>> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetByUsernameOrEmailAsync), OperationType.Get, "UsernameOrEmail");

            try
            {
                string lookupValue = usernameOrEmail.Trim();

                var user = await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        currentUser => currentUser.Username == lookupValue || currentUser.Email == lookupValue,
                        ct).ConfigureAwait(false);

                if (user is null)
                {
                    return Result<User>.Failure(
                        MediaVaultErrors.Unauthorized(baseErrorContext),
                        "Invalid username/email or password.");
                }

                return Result<User>.Success(user);
            }
            catch (OperationCanceledException)
            {
                return Result<User>.Failure(MediaVaultErrors.Cancelled(baseErrorContext));
            }
            catch (Exception ex)
            {
                return LogAndFail<User>(
                    DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex),
                    baseErrorContext);
            }
        }
    }
}
