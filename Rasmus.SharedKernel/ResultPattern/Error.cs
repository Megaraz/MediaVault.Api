namespace Rasmus.SharedKernel.ResultPattern
{
    public enum ErrorType
    {
        Failure,
        Validation,
        NotFound,
        Conflict,
        Unauthorized,
        Forbidden,
        None

    }

    public record Error(string Code, string Description, ErrorType Type)
    {

        public static Error None => new Error(string.Empty, string.Empty, ErrorType.None);

        public static Error DbCreateFailure<T>(string errorDescriptionPrefix, Exception? exception)
            => new Error(
                ErrorCode.Create<T>(ErrorCodes.DatabaseError.DbFailure).Code,
                $"{errorDescriptionPrefix}: A Database-Exception occurred while creating the entity in the database: {exception?.Message}",
                ErrorType.Failure);

        public static Error DbGetFailure<T>(string errorDescriptionPrefix, Exception? exception)
        {
            var errorCode = ErrorCode.Get<T>(ErrorCodes.DatabaseError.DbFailure);

            return new Error(
                errorCode.Code,
                $"{errorDescriptionPrefix}: A Database-Exception occurred while getting the entity {errorCode.NameOfEntity} from the database: {exception?.Message}",
                ErrorType.Failure);
        }

        public static Error DbGetCollectionFailure<T>(string errorDescriptionPrefix, Exception? exception)
        {
            var errorCode = ErrorCode.GetCollection<T>(ErrorCodes.DatabaseError.DbFailure);

            return new Error(
                errorCode.Code,
                $"{errorDescriptionPrefix}: A Database-Exception occurred while getting the list of entities {errorCode.NameOfEntity} from the database: {exception?.Message}",
                ErrorType.Failure);
        }

        public static Error DbDeleteFailure<T>(string errorDescriptionPrefix, Exception? exception)
        {
            var errorCode = ErrorCode.Delete<T>(ErrorCodes.DatabaseError.DbFailure);

            return new Error(
                errorCode.Code,
                $"{errorDescriptionPrefix}: A Database-Exception occurred while deleting the entity {errorCode.NameOfEntity} from the database: {exception?.Message}",
                ErrorType.Failure);
        }

        public static Error DbUpdateFailure<T>(string errorDescriptionPrefix, Exception? exception)
        {
            var errorCode = ErrorCode.Update<T>(ErrorCodes.DatabaseError.DbFailure);

            return new Error(
                errorCode.Code,
                $"{errorDescriptionPrefix}: A Database-Exception occurred while updating the entity {errorCode.NameOfEntity} in the database: {exception?.Message}",
                ErrorType.Failure);
        }

        public static Error NullValue<T>(string currentOperation, string errorDescriptionPrefix, out string errorMessageReason)
        {
            var errorCode = ErrorCode.NullValue<T>(currentOperation);

            errorMessageReason = $"{errorCode.NameOfEntity} cannot be null or default";

            // Create full error of ErrorType.Validation, with ErrorCode from above, and return it
            return new Error(
                errorCode.Code,
                $"{errorDescriptionPrefix}: {errorMessageReason}",
                ErrorType.Validation);

        }

        public static Error NotFound<T>(string errorDescriptionPrefix)
        {
            var errorCode = ErrorCode.Get<T>(ErrorCodes.GeneralError.NotFound);

            return new Error(errorCode.Code, $"{errorDescriptionPrefix}: {errorCode.NameOfEntity} not found", ErrorType.NotFound);

        }

        public static Error Validation(ErrorCode code, string description) =>
            new(code.Code, description, ErrorType.Validation);

        public static Error Conflict(ErrorCode code, string description) =>
            new(code.Code, description, ErrorType.Conflict);

        public static Error Unauthorized(ErrorCode code, string description) =>
            new(code.Code, description, ErrorType.Unauthorized);

        public static Error Forbidden(ErrorCode code, string description) =>
            new(code.Code, description, ErrorType.Forbidden);

        public static Error Failure(ErrorCode code, string description) =>
            new(code.Code, description, ErrorType.Failure);

    }

}