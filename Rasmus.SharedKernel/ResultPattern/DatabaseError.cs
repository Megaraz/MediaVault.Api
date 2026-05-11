namespace Rasmus.SharedKernel.ResultPattern
{
    public enum DatabaseErrorType
    {
        Custom = 0,
        CreateFailure = 1,
        GetFailure = 2,
        GetCollectionFailure = 3,
        UpdateFailure = 4,
        DeleteFailure = 5,
        ConcurrencyFailure = 6,
        UnexpectedFailure = 7
    }

    public record DatabaseError : Error
    {
        public DatabaseErrorType DatabaseErrorType { get; }

        private DatabaseError(string code, string description, DatabaseErrorType type, string userMessage, Exception? exception = null)
            : base(code, description, ErrorType.Database, userMessage, exception)
        {
            DatabaseErrorType = type;
        }

        public static DatabaseError CreateFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseFailure);

            string defaultDescriptionSuffix = $"A database failure occurred while creating the entity {errorCode.NameOfEntity} in the database.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new DatabaseError(errorCode.Code, formattedErrorDescription, DatabaseErrorType.CreateFailure, defaultDescriptionSuffix, exception);
        }

        public static DatabaseError GetFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseFailure);

            string defaultDescriptionSuffix = $"A database failure occurred while getting the entity {errorCode.NameOfEntity} from the database.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new DatabaseError(errorCode.Code, formattedErrorDescription, DatabaseErrorType.GetFailure, defaultDescriptionSuffix, exception);
        }

        public static DatabaseError GetCollectionFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseFailure);

            string defaultDescriptionSuffix = $"A database failure occurred while getting the list of entities {errorCode.NameOfEntity} from the database.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new DatabaseError(errorCode.Code, formattedErrorDescription, DatabaseErrorType.GetCollectionFailure, defaultDescriptionSuffix, exception);
        }

        public static DatabaseError DeleteFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseFailure);

            string defaultDescriptionSuffix = $"A database failure occurred while deleting the entity {errorCode.NameOfEntity} from the database.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new DatabaseError(errorCode.Code, formattedErrorDescription, DatabaseErrorType.DeleteFailure, defaultDescriptionSuffix, exception);
        }

        public static DatabaseError UpdateFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseFailure);

            string defaultDescriptionSuffix = $"A database failure occurred while updating the entity {errorCode.NameOfEntity} in the database.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new DatabaseError(errorCode.Code, formattedErrorDescription, DatabaseErrorType.UpdateFailure, defaultDescriptionSuffix, exception);
        }

        public static DatabaseError ConcurrencyFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseConcurrencyFailure);

            string defaultDescriptionSuffix = $"A concurrency conflict occurred while processing {errorCode.NameOfEntity}. The entity was modified or deleted by another process.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new DatabaseError(errorCode.Code, formattedErrorDescription, DatabaseErrorType.ConcurrencyFailure, defaultDescriptionSuffix, exception);
        }
        public static DatabaseError UnexpectedFailure(
            ErrorContext errorContext,
            Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseFailure);

            string defaultDescriptionSuffix =
                $"An unexpected infrastructure failure occurred while performing {errorContext.Operation} for entity {errorCode.NameOfEntity}.";

            string formattedErrorDescription =
                FormatDescription(errorContext, defaultDescriptionSuffix);

            return new DatabaseError(
                errorCode.Code,
                formattedErrorDescription,
                DatabaseErrorType.UnexpectedFailure,
                defaultDescriptionSuffix,
                exception);
        }
    }
}