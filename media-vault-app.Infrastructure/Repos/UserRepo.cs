using System.Data.Common;
using media_vault_app.Application.Identity;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
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
            CanonicalizeUser(entity);

            try
            {
                _dbSet.Add(entity);
                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
                return Result.Success();
            }
            catch (DbUpdateException dbEx)
            {
                var baseErrorContext = DefineErrorContext(nameof(RegisterUserAsync), OperationType.Create);

                if (IsUniqueConstraintViolation(dbEx))
                {
                    return Result.Failure(MediaVaultErrors.Conflict(baseErrorContext));
                }

                return LogAndFail(
                    DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, dbEx),
                    baseErrorContext);
            }
        }

        public async Task<Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>> CheckRegistrationAvailabilityAsync(string username, string email, CancellationToken ct = default)
        {

            try
            {
                string lookupUsername = UserIdentifierCanonicalizer.CanonicalizeUsername(username);
                string lookupEmail = UserIdentifierCanonicalizer.CanonicalizeEmail(email);

                var matchingUsers = await _dbSet
                    .AsNoTracking()
                    .Where(currentUser => currentUser.Username == lookupUsername || currentUser.Email == lookupEmail)
                    .Select(currentUser => new { currentUser.Username, currentUser.Email })
                    .ToListAsync(ct).ConfigureAwait(false);

                bool usernameExists = matchingUsers.Any(currentUser =>
                    UserIdentifierCanonicalizer.CanonicalizeUsername(currentUser.Username) == lookupUsername);
                bool emailExists = matchingUsers.Any(currentUser =>
                    UserIdentifierCanonicalizer.CanonicalizeEmail(currentUser.Email) == lookupEmail);

                return Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.Success((!usernameExists, !emailExists));
            }
            catch (DbException ex)
            {
                var baseErrorContext = DefineErrorContext(nameof(CheckRegistrationAvailabilityAsync), OperationType.Get);
                return LogAndFail<(bool IsUserNameAvailable, bool IsEmailAvailable)>(
                    DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex),
                    baseErrorContext);
            }
        }

        public async Task<Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>> CheckProfileUpdateAvailabilityAsync(
            Guid userId,
            string username,
            string email,
            CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(CheckProfileUpdateAvailabilityAsync), OperationType.Update);

            try
            {
                string lookupUsername = UserIdentifierCanonicalizer.CanonicalizeUsername(username);
                string lookupEmail = UserIdentifierCanonicalizer.CanonicalizeEmail(email);

                var matchingUsers = await _dbSet
                    .AsNoTracking()
                    .Where(currentUser =>
                        currentUser.Id != userId &&
                        (currentUser.Username == lookupUsername ||
                         currentUser.Email == lookupEmail))
                    .Select(currentUser => new { currentUser.Username, currentUser.Email })
                    .ToListAsync(ct)
                    .ConfigureAwait(false);

                bool usernameExists = matchingUsers.Any(
                    currentUser =>
                        UserIdentifierCanonicalizer.CanonicalizeUsername(currentUser.Username) == lookupUsername);
                bool emailExists = matchingUsers.Any(
                    currentUser =>
                        UserIdentifierCanonicalizer.CanonicalizeEmail(currentUser.Email) == lookupEmail);

                return Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.Success(
                    (!usernameExists, !emailExists));
            }
            catch (DbException ex)
            {
                return LogAndFail<(bool IsUserNameAvailable, bool IsEmailAvailable)>(
                    DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex),
                    baseErrorContext);
            }
        }

        public async Task<Result> UpdateProfileAsync(
            Guid userId,
            string username,
            string email,
            CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(UpdateProfileAsync), OperationType.Update);

            try
            {
                var user = await _dbSet
                    .FindAsync(new object[] { userId }, ct)
                    .ConfigureAwait(false);

                if (user is null)
                {
                    return Result.Failure(
                        MediaVaultErrors.NotFound(baseErrorContext));
                }

                user.Username = UserIdentifierCanonicalizer.CanonicalizeUsername(username);
                user.Email = UserIdentifierCanonicalizer.CanonicalizeEmail(email);
                user.UpdatedAtUtc = DateTime.UtcNow;

                await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return LogAndFail(
                    DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext, ex),
                    baseErrorContext);
            }
            catch (DbUpdateException ex)
            {
                if (IsUniqueConstraintViolation(ex))
                    return Result.Failure(MediaVaultErrors.Conflict(baseErrorContext));

                return LogAndFail(
                    DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, ex),
                    baseErrorContext);
            }
            catch (DbException ex)
            {
                return LogAndFail(
                    DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex),
                    baseErrorContext);
            }
        }

        public async Task<Result<User>> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default)
        {
            var baseErrorContext = DefineErrorContext(nameof(GetByUsernameOrEmailAsync), OperationType.Get, "UsernameOrEmail");

            try
            {
                string lookupValue = UserIdentifierCanonicalizer.CanonicalizeLoginIdentifier(usernameOrEmail);

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
            catch (DbException ex)
            {
                return LogAndFail<User>(
                    DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex),
                    baseErrorContext);
            }
        }

        private static void CanonicalizeUser(User user)
        {
            user.Username = UserIdentifierCanonicalizer.CanonicalizeUsername(user.Username);
            user.Email = UserIdentifierCanonicalizer.CanonicalizeEmail(user.Email);
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
            exception.InnerException switch
            {
                SqliteException sqliteException =>
                    sqliteException.SqliteErrorCode == 19 &&
                    sqliteException.SqliteExtendedErrorCode == 2067,
                DbException dbException =>
                    dbException.ErrorCode is 2601 or 2627,
                _ => false
            };
    }
}
