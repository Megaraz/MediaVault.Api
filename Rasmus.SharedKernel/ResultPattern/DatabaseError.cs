namespace Rasmus.SharedKernel.ResultPattern
{
    public enum DatabaseErrorType
    {
        Custom = 0,
        SaveChangesFailure = 1,
        ConcurrencyFailure = 2,
        QueryFailure = 3,
        UnexpectedFailure = 4
    }

    public record DatabaseError : Error
    {
        public DatabaseErrorType DatabaseErrorType { get; }

        private DatabaseError(string code, string description, DatabaseErrorType type, string userMessage, Exception? exception = null)
            : base(code, description, ErrorType.Database, userMessage, exception)
        {
            DatabaseErrorType = type;
        }

        public static DatabaseError SaveChangesFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseSaveChangesFailure);

            string defaultDescriptionSuffix = $"A database failure occurred while saving changes for {errorCode.NameOfEntity}.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new DatabaseError(errorCode.Code, formattedErrorDescription, DatabaseErrorType.SaveChangesFailure, defaultDescriptionSuffix, exception);
        }

        public static DatabaseError QueryFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseQueryFailure);

            string defaultDescriptionSuffix = $"A database failure occurred while querying {errorCode.NameOfEntity}.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new DatabaseError(errorCode.Code, formattedErrorDescription, DatabaseErrorType.QueryFailure, defaultDescriptionSuffix, exception);
        }

        public static DatabaseError ConcurrencyFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseConcurrencyFailure);

            string defaultDescriptionSuffix = $"A concurrency conflict occurred while processing {errorCode.NameOfEntity}. The entity was modified or deleted by another process.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new DatabaseError(errorCode.Code, formattedErrorDescription, DatabaseErrorType.ConcurrencyFailure, defaultDescriptionSuffix, exception);
        }

        public static DatabaseError UnexpectedFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseUnexpectedFailure);

            string defaultDescriptionSuffix =
                $"An unexpected infrastructure failure occurred while performing {errorContext.Operation} for entity {errorCode.NameOfEntity}.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new DatabaseError(errorCode.Code, formattedErrorDescription, DatabaseErrorType.UnexpectedFailure, defaultDescriptionSuffix, exception);
        }
    }
}