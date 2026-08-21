using System.Data.Common;
using System.Runtime.CompilerServices;
using media_vault_app.Application.Identity;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;
using media_vault_app.Infrastructure.Diagnostics;
using media_vault_app.Infrastructure.Timestamps;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;

namespace media_vault_app.Infrastructure.Repos;

public class UserRepo : IUserRepo
{
    private readonly AppDbContext _appDbContext;
    private readonly DbSet<User> _dbSet;
    private readonly ErrorEventLogger<UserRepo> _errorEventLogger;
    private readonly ServerTimestampPolicy _timestampPolicy;

    public UserRepo(
        AppDbContext appDbContext,
        ErrorEventLogger<UserRepo> errorEventLogger,
        ServerTimestampPolicy? timestampPolicy = null)
    {
        _appDbContext = appDbContext;
        _dbSet = appDbContext.Set<User>();
        _errorEventLogger = errorEventLogger;
        _timestampPolicy = timestampPolicy ?? new ServerTimestampPolicy(TimeProvider.System);
    }

    public async Task<Result<User>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var baseErrorContext = DefineErrorContext(nameof(GetByIdAsync), OperationType.Get);

        try
        {
            var user = await _dbSet.FindAsync([id], ct).ConfigureAwait(false);
            if (user is null)
            {
                return Result<User>.Failure(MediaVaultErrors.NotFound(baseErrorContext));
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

    public async Task<Result<bool>> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        var baseErrorContext = DefineErrorContext(nameof(ExistsAsync), OperationType.Get);

        try
        {
            var exists = await _dbSet.AnyAsync(user => user.Id == id, ct).ConfigureAwait(false);
            if (!exists)
            {
                return Result<bool>.Failure(MediaVaultErrors.NotFound(baseErrorContext));
            }

            return Result<bool>.Success(true);
        }
        catch (DbException ex)
        {
            return LogAndFail<bool>(
                DatabaseFailurePolicy.QueryFailure(baseErrorContext, ex),
                baseErrorContext);
        }
    }

    public async Task<Result> RegisterUserAsync(User entity, CancellationToken ct = default)
    {
        CanonicalizeUser(entity);

        try
        {
            _timestampPolicy.Initialize(entity);
            _dbSet.Add(entity);
            await _appDbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result.Success();
        }
        catch (DbUpdateException dbEx)
        {
            var baseErrorContext = DefineErrorContext(nameof(RegisterUserAsync), OperationType.Create);

            if (IsUniqueConstraintViolation(dbEx))
                return Result.Failure(MediaVaultErrors.Conflict(baseErrorContext));

            return LogAndFail(
                DatabaseFailurePolicy.SaveChangesFailure(baseErrorContext, dbEx),
                baseErrorContext);
        }
    }

    public async Task<Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>> CheckRegistrationAvailabilityAsync(
        string username,
        string email,
        CancellationToken ct = default)
    {
        try
        {
            var lookupUsername = UserIdentifierCanonicalizer.CanonicalizeUsername(username);
            var lookupEmail = UserIdentifierCanonicalizer.CanonicalizeEmail(email);
            var matchingUsers = await _dbSet
                .AsNoTracking()
                .Where(user => user.Username == lookupUsername || user.Email == lookupEmail)
                .Select(user => new { user.Username, user.Email })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            var usernameExists = matchingUsers.Any(user =>
                UserIdentifierCanonicalizer.CanonicalizeUsername(user.Username) == lookupUsername);
            var emailExists = matchingUsers.Any(user =>
                UserIdentifierCanonicalizer.CanonicalizeEmail(user.Email) == lookupEmail);

            return Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.Success(
                (!usernameExists, !emailExists));
        }
        catch (DbException ex)
        {
            var baseErrorContext = DefineErrorContext(
                nameof(CheckRegistrationAvailabilityAsync),
                OperationType.Get);
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
        var baseErrorContext = DefineErrorContext(
            nameof(CheckProfileUpdateAvailabilityAsync),
            OperationType.Update);

        try
        {
            var lookupUsername = UserIdentifierCanonicalizer.CanonicalizeUsername(username);
            var lookupEmail = UserIdentifierCanonicalizer.CanonicalizeEmail(email);
            var matchingUsers = await _dbSet
                .AsNoTracking()
                .Where(user =>
                    user.Id != userId &&
                    (user.Username == lookupUsername || user.Email == lookupEmail))
                .Select(user => new { user.Username, user.Email })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            var usernameExists = matchingUsers.Any(user =>
                UserIdentifierCanonicalizer.CanonicalizeUsername(user.Username) == lookupUsername);
            var emailExists = matchingUsers.Any(user =>
                UserIdentifierCanonicalizer.CanonicalizeEmail(user.Email) == lookupEmail);

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
        int expectedVersion,
        CancellationToken ct = default)
    {
        var baseErrorContext = DefineErrorContext(nameof(UpdateProfileAsync), OperationType.Update);

        try
        {
            var user = await _dbSet.FindAsync([userId], ct).ConfigureAwait(false);
            if (user is null)
            {
                return Result.Failure(MediaVaultErrors.NotFound(baseErrorContext));
            }

            if (user.Version != expectedVersion)
            {
                return LogAndFail(
                    DatabaseFailurePolicy.ConcurrencyFailure(baseErrorContext),
                    baseErrorContext);
            }

            user.Username = UserIdentifierCanonicalizer.CanonicalizeUsername(username);
            user.Email = UserIdentifierCanonicalizer.CanonicalizeEmail(email);
            var hasMeaningfulChanges = ApplyUpdateTimestamp(
                user,
                user.CreatedAtUtc,
                user.UpdatedAtUtc);
            var versionProperty = _appDbContext.Entry(user).Property(entry => entry.Version);
            versionProperty.OriginalValue = expectedVersion;
            versionProperty.CurrentValue = hasMeaningfulChanges
                ? checked(expectedVersion + 1)
                : expectedVersion;
            versionProperty.IsModified = hasMeaningfulChanges;

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

    public async Task<Result> DeleteAccountAsync(Guid userId, CancellationToken ct = default)
    {
        var baseErrorContext = DefineErrorContext(nameof(DeleteAccountAsync), OperationType.Delete);

        try
        {
            var user = await _dbSet.FindAsync([userId], ct).ConfigureAwait(false);
            if (user is null)
            {
                return Result.Failure(MediaVaultErrors.NotFound(baseErrorContext));
            }

            _dbSet.Remove(user);
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

    public async Task<Result<User>> GetByUsernameOrEmailAsync(
        string usernameOrEmail,
        CancellationToken ct = default)
    {
        var baseErrorContext = DefineErrorContext(
            nameof(GetByUsernameOrEmailAsync),
            OperationType.Get,
            "UsernameOrEmail");

        try
        {
            var lookupValue = UserIdentifierCanonicalizer.CanonicalizeLoginIdentifier(usernameOrEmail);
            var user = await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    currentUser => currentUser.Username == lookupValue || currentUser.Email == lookupValue,
                    ct)
                .ConfigureAwait(false);

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

    private bool ApplyUpdateTimestamp(
        User user,
        DateTime originalCreatedAtUtc,
        DateTime originalUpdatedAtUtc)
    {
        _appDbContext.ChangeTracker.DetectChanges();
        var entry = _appDbContext.Entry(user);
        var hasMeaningfulChanges = entry.Properties.Any(
            property => property.IsModified &&
                property.Metadata.Name is not nameof(User.CreatedAtUtc) and
                not nameof(User.UpdatedAtUtc));

        _timestampPolicy.ApplyUpdate(
            user,
            originalCreatedAtUtc,
            originalUpdatedAtUtc,
            hasMeaningfulChanges);

        entry.Property(nameof(User.CreatedAtUtc)).IsModified = false;
        entry.Property(nameof(User.UpdatedAtUtc)).IsModified = hasMeaningfulChanges;

        return hasMeaningfulChanges;
    }

    private Result LogAndFail(
        Error error,
        ErrorContext errorContext,
        [CallerMemberName] string methodName = "")
    {
        var context = new ErrorEventContext(
            "Infrastructure",
            GetType().Name,
            methodName,
            errorContext);
        _errorEventLogger.Log(error, context);
        return Result.Failure(error);
    }

    private Result<T> LogAndFail<T>(
        Error error,
        ErrorContext errorContext,
        [CallerMemberName] string methodName = "")
        where T : notnull
    {
        var context = new ErrorEventContext(
            "Infrastructure",
            GetType().Name,
            methodName,
            errorContext);
        _errorEventLogger.Log(error, context);
        return Result<T>.Failure(error);
    }

    private static ErrorContext DefineErrorContext(
        string methodName,
        OperationType operation,
        string? fieldName = null) =>
        new(operation: operation, entityName: nameof(User), fieldName: fieldName);

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
            DbException dbException => dbException.ErrorCode is 2601 or 2627,
            _ => false
        };
}
