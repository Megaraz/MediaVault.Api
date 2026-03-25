namespace Rasmus.SharedKernel.ResultPattern
{
    public enum ErrorType
    {
        None = 0,
        Failure = 1,
        Validation = 2,
        NotFound = 3,
        Conflict = 4,
        Unauthorized = 5,
        Forbidden = 6,
        Database = 7,

    }

    public record Error(string Code, string Description, ErrorType Type, Exception? exception = null)
    {

        public static readonly Error None = new Error(string.Empty, string.Empty, ErrorType.None);

        public static Error DbCreateFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseFailure);

            string defaultDescriptionSuffix = $"A database failure occurred while creating the entity {errorCode.NameOfEntity} in the database.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new Error(errorCode.Code, formattedErrorDescription, ErrorType.Database, exception);
        }

        public static Error DbGetFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseFailure);

            string defaultDescriptionSuffix = $"A database failure occurred while getting the entity {errorCode.NameOfEntity} from the database.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new Error(errorCode.Code, formattedErrorDescription, ErrorType.Database, exception);
        }

        public static Error DbGetCollectionFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseFailure);

            string defaultDescriptionSuffix = $"A database failure occurred while getting the list of entities {errorCode.NameOfEntity} from the database.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new Error(errorCode.Code, formattedErrorDescription, ErrorType.Database, exception);
        }

        public static Error DbDeleteFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseFailure);

            string defaultDescriptionSuffix = $"A database failure occurred while deleting the entity {errorCode.NameOfEntity} from the database.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new Error(errorCode.Code, formattedErrorDescription, ErrorType.Database, exception);
        }

        public static Error DbUpdateFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.DatabaseFailure);

            string defaultDescriptionSuffix = $"A database failure occurred while updating the entity {errorCode.NameOfEntity} in the database.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new Error(errorCode.Code, formattedErrorDescription, ErrorType.Database, exception);
        }

        public static Error NotFound(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.GeneralNotFound);

            string defaultDescriptionSuffix = $"{errorContext.EntityName} not found";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new Error(errorCode.Code, formattedErrorDescription, ErrorType.NotFound);
        }

        public static Error Conflict(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.GeneralConflict);

            string defaultDescriptionSuffix = $"Unique {errorContext.EntityName} constraint violated.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new Error(errorCode.Code, formattedErrorDescription, ErrorType.Conflict);
        }

        public static Error Unauthorized(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.GeneralUnauthorized);

            string defaultDescriptionSuffix = $"Unauthorized access" + (string.IsNullOrWhiteSpace(errorContext.FieldName) ? "" : $" to {errorContext.FieldName}");

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new Error(errorCode.Code, formattedErrorDescription, ErrorType.Unauthorized);
        }


        //public static Error Forbidden(ErrorCode code, string description) =>
        //    new(code.Code, description, ErrorType.Forbidden);

        //public static Error Failure(ErrorCode code, string description) =>
        //    new(code.Code, description, ErrorType.Failure);

        protected static string FormatDescription(ErrorContext errorContext, string defaultDescriptionSuffix)
        {
            if (string.IsNullOrWhiteSpace(errorContext.DescriptionSuffix))
                errorContext.DescriptionSuffix = defaultDescriptionSuffix;

            return $"{errorContext.DescriptionPrefix}: {errorContext.DescriptionSuffix}";
        }

    }

}