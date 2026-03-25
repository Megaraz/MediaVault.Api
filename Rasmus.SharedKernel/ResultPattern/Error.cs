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
            => new Error(
                ErrorCode.For(OperationType.Create, errorContext.EntityName, ErrorReasonCode.DatabaseFailure).Code,
                $"{errorContext.DescriptionSuffix}: A Database-Exception occurred while creating the entity in the database",
                ErrorType.Database,
                exception);

        public static Error DbGetFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(OperationType.Get, errorContext.EntityName, ErrorReasonCode.DatabaseFailure);

            return new Error(
                errorCode.Code,
                $"{errorContext.DescriptionSuffix}: A Database failure occurred while getting the entity {errorCode.NameOfEntity} from the database",
                ErrorType.Database,
                exception);
        }

        public static Error DbGetCollectionFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(OperationType.GetCollection, errorContext.EntityName, ErrorReasonCode.DatabaseFailure);

            return new Error(
                errorCode.Code,
                $"{errorContext.DescriptionSuffix}: A Database-Exception occurred while getting the list of entities {errorCode.NameOfEntity} from the database",
                ErrorType.Database,
                exception);
        }

        public static Error DbDeleteFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(OperationType.Delete, errorContext.EntityName, ErrorReasonCode.DatabaseFailure);

            return new Error(
                errorCode.Code,
                $"{errorContext.DescriptionSuffix}: A Database-Exception occurred while deleting the entity {errorCode.NameOfEntity} from the database",
                ErrorType.Database,
                exception);
        }

        public static Error DbUpdateFailure(ErrorContext errorContext, Exception exception)
        {
            var errorCode = ErrorCode.For(OperationType.Update, errorContext.EntityName, ErrorReasonCode.DatabaseFailure);

            return new Error(
                errorCode.Code,
                $"{errorContext.DescriptionSuffix}: A Database-Exception occurred while updating the entity {errorCode.NameOfEntity} in the database",
                ErrorType.Database,
                exception);
        }

        public static Error NotFound<T>(string errorDescriptionPrefix) =>
            new Error(ErrorCode.For<T>(OperationType.Get, ErrorReasonCode.GeneralNotFound).Code,
                    $"{errorDescriptionPrefix}: {typeof(T).Name} not found",
                    ErrorType.NotFound);

        public static Error Conflict<T>(OperationType operation, string errorDescriptionPrefix) =>
            new Error(ErrorCode.For<T>(operation, ErrorReasonCode.GeneralConflict).Code,
                    $"{errorDescriptionPrefix}: A conflict occurred during the {operation} operation.",
                    ErrorType.Conflict);

        public static Error Unauthorized(ErrorContext errorContext) =>
            new Error(
                ErrorCode.For(errorContext.Operation, errorContext.EntityName, ErrorReasonCode.GeneralUnauthorized).Code,
                $"{errorContext.DescriptionPrefix}: Unauthorized access.",
                ErrorType.Unauthorized);


        //public static Error Forbidden(ErrorCode code, string description) =>
        //    new(code.Code, description, ErrorType.Forbidden);

        //public static Error Failure(ErrorCode code, string description) =>
        //    new(code.Code, description, ErrorType.Failure);

    }

}