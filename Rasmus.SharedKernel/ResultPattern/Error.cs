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

    public record Error(string Code, string Description, ErrorType Type)
    {

        public static readonly Error None = new Error(string.Empty, string.Empty, ErrorType.None);

        public static Error DbCreateFailure<T>(string errorDescriptionPrefix, Exception? exception)
            => new Error(
                ErrorCode.For<T>(OperationType.Create, ErrorReasonCode.DatabaseFailure).Code,
                $"{errorDescriptionPrefix}: A Database-Exception occurred while creating the entity in the database: {exception?.Message}",
                ErrorType.Database);

        public static Error DbGetFailure<T>(string errorDescriptionPrefix, Exception? exception)
        {
            var errorCode = ErrorCode.For<T>(OperationType.Get, ErrorReasonCode.DatabaseFailure);

            return new Error(
                errorCode.Code,
                $"{errorDescriptionPrefix}: A Database-Exception occurred while getting the entity {errorCode.NameOfEntity} from the database: {exception?.Message}",
                ErrorType.Database);
        }

        public static Error DbGetCollectionFailure<T>(string errorDescriptionPrefix, Exception? exception)
        {
            var errorCode = ErrorCode.For<T>(OperationType.GetCollection, ErrorReasonCode.DatabaseFailure);

            return new Error(
                errorCode.Code,
                $"{errorDescriptionPrefix}: A Database-Exception occurred while getting the list of entities {errorCode.NameOfEntity} from the database: {exception?.Message}",
                ErrorType.Database);
        }

        public static Error DbDeleteFailure<T>(string errorDescriptionPrefix, Exception? exception)
        {
            var errorCode = ErrorCode.For<T>(OperationType.Delete, ErrorReasonCode.DatabaseFailure);

            return new Error(
                errorCode.Code,
                $"{errorDescriptionPrefix}: A Database-Exception occurred while deleting the entity {errorCode.NameOfEntity} from the database: {exception?.Message}",
                ErrorType.Database);
        }

        public static Error DbUpdateFailure<T>(string errorDescriptionPrefix, Exception? exception)
        {
            var errorCode = ErrorCode.For<T>(OperationType.Update, ErrorReasonCode.DatabaseFailure);

            return new Error(
                errorCode.Code,
                $"{errorDescriptionPrefix}: A Database-Exception occurred while updating the entity {errorCode.NameOfEntity} in the database: {exception?.Message}",
                ErrorType.Database);
        }

        public static Error NotFound<T>(string errorDescriptionPrefix) =>
            new Error(ErrorCode.For<T>(OperationType.Get, ErrorReasonCode.GeneralNotFound).Code,
                    $"{errorDescriptionPrefix}: {typeof(T).Name} not found",
                    ErrorType.NotFound);

        public static Error Conflict<T>(OperationType operation, string errorDescriptionPrefix) =>
            new Error(ErrorCode.For<T>(operation, ErrorReasonCode.GeneralConflict).Code,
                    $"{errorDescriptionPrefix}: A conflict occurred during the {operation} operation.",
                    ErrorType.Conflict);

        //public static Error Unauthorized(ErrorCode code, string description) =>
        //    new(code.Code, description, ErrorType.Unauthorized);

        //public static Error Forbidden(ErrorCode code, string description) =>
        //    new(code.Code, description, ErrorType.Forbidden);

        //public static Error Failure(ErrorCode code, string description) =>
        //    new(code.Code, description, ErrorType.Failure);

    }

}