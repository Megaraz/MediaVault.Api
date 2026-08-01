using Megaraz.ResultPattern;
using Megaraz.ResultPattern.Infrastructure;

namespace media_vault_app.Infrastructure.Diagnostics;

internal static class DatabaseFailurePolicy
{
    public static DatabaseError SaveChangesFailure(ErrorContext context, Exception exception) =>
        DatabaseError.SaveChangesFailure(
            context,
            exception,
            $"A database failure occurred while saving changes for {context.EntityName}.");

    public static DatabaseError QueryFailure(ErrorContext context, Exception exception) =>
        DatabaseError.QueryFailure(
            context,
            exception,
            $"A database failure occurred while querying {context.EntityName}.");

    public static DatabaseError ConcurrencyFailure(ErrorContext context, Exception exception) =>
        DatabaseError.ConcurrencyFailure(
            context,
            exception,
            $"A concurrency conflict occurred while processing {context.EntityName}. The entity was modified or deleted by another process.");

    public static DatabaseError UnexpectedFailure(ErrorContext context, Exception exception) =>
        DatabaseError.UnexpectedFailure(
            context,
            exception,
            $"An unexpected infrastructure failure occurred while performing {context.Operation} for entity {context.EntityName}.");
}
