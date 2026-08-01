using Megaraz.ResultPattern;

namespace Rasmus.SharedKernel.ResultPatternCompatibility;

/// <summary>
/// Temporary adapter that keeps the legacy database classification until issue #93 adopts
/// Megaraz.ResultPattern.Infrastructure. It deliberately uses package core Error types.
/// </summary>
public enum DatabaseErrorType
{
    Custom = 0,
    SaveChangesFailure = 1,
    ConcurrencyFailure = 2,
    QueryFailure = 3,
    UnexpectedFailure = 4
}

public record DatabaseError : Megaraz.ResultPattern.Error
{
    public DatabaseErrorType DatabaseErrorType { get; }

    private DatabaseError(
        string code,
        string description,
        DatabaseErrorType type,
        string userMessage,
        Exception? exception = null)
        : base(code, description, Megaraz.ResultPattern.ErrorType.External, userMessage, exception)
    {
        DatabaseErrorType = type;
    }

    public static DatabaseError SaveChangesFailure(ErrorContext context, Exception exception) =>
        Create(context, "DbSaveChangesFailure", DatabaseErrorType.SaveChangesFailure,
            $"A database failure occurred while saving changes for {context.EntityName}.", exception);

    public static DatabaseError QueryFailure(ErrorContext context, Exception exception) =>
        Create(context, "DbQueryFailure", DatabaseErrorType.QueryFailure,
            $"A database failure occurred while querying {context.EntityName}.", exception);

    public static DatabaseError ConcurrencyFailure(ErrorContext context, Exception exception) =>
        Create(context, "DbConcurrencyFailure", DatabaseErrorType.ConcurrencyFailure,
            $"A concurrency conflict occurred while processing {context.EntityName}. The entity was modified or deleted by another process.", exception);

    public static DatabaseError UnexpectedFailure(ErrorContext context, Exception exception) =>
        Create(context, "DbUnexpectedFailure", DatabaseErrorType.UnexpectedFailure,
            $"An unexpected infrastructure failure occurred while performing {context.Operation} for entity {context.EntityName}.", exception);

    private static DatabaseError Create(
        ErrorContext context,
        string reason,
        DatabaseErrorType type,
        string message,
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(exception);
        var code = Megaraz.ResultPattern.ErrorCode.For(context, reason).Code;
        var description = TemporaryResultPatternBridge.FormatDescription(context, message);
        return new DatabaseError(code, description, type, message, exception);
    }
}
