namespace Rasmus.SharedKernel.ResultPattern
{
    public enum ErrorType
    {
        Failure,
        Validation,
        NotFound,
        Conflict,
        Unauthorized,
        Forbidden
    }

    public static class ErrorCodes
    {
        public static class Operation
        {
            public const string Create = "Create";
            public const string Update = "Update";
            public const string Delete = "Delete";
            public const string Get = "Get";
            public const string List = "List";
        }

        public static class Entity
        {
            public const string User = "User";
            public const string MediaEntry = "MediaEntry";
            public const string BookEntry = "BookEntry";
            public const string MovieEntry = "MovieEntry";
            public const string SeriesEntry = "SeriesEntry";
            public const string AnimeEntry = "AnimeEntry";
            public const string MangaEntry = "MangaEntry";
            public const string TvShowEntry = "TvShowEntry";
            public const string GameEntry = "GameEntry";
            public const string Season = "Season";
            public const string Episode = "Episode";
            public const string Author = "Author";
        }

        public static class ValidationError
        {
            public const string Required = "Required";
            public const string InvalidFormat = "InvalidFormat";
            public const string TooShort = "TooShort";
            public const string TooLong = "TooLong";
            public const string OutOfRange = "OutOfRange";

        }

        public static class DatabaseError
        {
            public const string DbCreateException = "DbCreateException";
            public const string DbGetException = "DbGetException";
            public const string DbListException = "DbListException";
            public const string DbUpdateException = "DbUpdateException";
            public const string DbDeleteException = "DbDeleteException";
        }


        public static class GeneralError
        {
            public const string InvalidInput = "InvalidInput";
            public const string NotFound = "NotFound";
            public const string Conflict = "Conflict";
            public const string Unauthorized = "Unauthorized";
            public const string Forbidden = "Forbidden";
            public const string Failure = "Failure";
        }
    }

    public sealed record ErrorCode
    {
        public string Operation { get; }
        public string NameOfEntity { get; }
        public string Error { get; }

        public string Code { get; }

        private ErrorCode(string operation, string nameOfEntity, string error)
        {
            Operation = operation;
            NameOfEntity = nameOfEntity;
            Error = error;

            Code = $"{Operation}.{NameOfEntity}.{Error}";
        }

        public static ErrorCode Create<T>(string error) =>
            new(ErrorCodes.Operation.Create, typeof(T).Name, error);

        public static ErrorCode Get<T>(string error, out string entityName) =>
            new(ErrorCodes.Operation.Get, entityName = typeof(T).Name, error);

        public static ErrorCode List<T>(string error, out string entityName) =>
            new(ErrorCodes.Operation.List, entityName = typeof(T).Name, error);

        public static ErrorCode Delete<T>(string error, out string entityName) =>
            new(ErrorCodes.Operation.Delete, entityName = typeof(T).Name, error);

        public static ErrorCode Update<T>(string error, out string entityName) =>
            new(ErrorCodes.Operation.Update, entityName = typeof(T).Name, error);

        public static ErrorCode NullValue<T>(string currentOperation, out string entityName) =>
            new(currentOperation, entityName = typeof(T).Name, ErrorCodes.ValidationError.Required);


        public override string ToString()
        {
            return $"{Operation}.{NameOfEntity}.{Error}";

        }



    }

    public sealed record Error(string Code, string Description, ErrorType ErrorType)
    {
        public static Error DbCreateException<T>(string errorDescriptionPrefix, Exception? exception)
            => new Error(
                ErrorCode.Create<T>(ErrorCodes.DatabaseError.DbCreateException).Code,
                $"{errorDescriptionPrefix}: A Database-Exception occurred while creating the entity in the database: {exception?.Message}",
                ErrorType.Failure);

        public static Error DbGetException<T>(string errorDescriptionPrefix, Exception? exception)
            => new Error(
                ErrorCode.Get<T>(ErrorCodes.DatabaseError.DbGetException, out string entityName).Code,
                $"{errorDescriptionPrefix}: A Database-Exception occurred while getting the entity {entityName} from the database: {exception?.Message}",
                ErrorType.Failure);

        public static Error DbListException<T>(string errorDescriptionPrefix, Exception? exception)
            => new Error(
                    ErrorCode.List<T>(ErrorCodes.DatabaseError.DbListException, out string entityName).Code,
                $"{errorDescriptionPrefix}: A Database-Exception occurred while getting the list of entities {entityName} from the database: {exception?.Message}",
                ErrorType.Failure);

        public static Error DbDeleteException<T>(string errorDescriptionPrefix, Exception? exception)
            => new Error(
                ErrorCode.Delete<T>(ErrorCodes.DatabaseError.DbDeleteException, out string entityName).Code,
                $"{errorDescriptionPrefix}: A Database-Exception occurred while deleting the entity {entityName} from the database: {exception?.Message}",
                ErrorType.Failure);

        public static Error DbUpdateException<T>(string errorDescriptionPrefix, Exception? exception)
            => new Error(
                ErrorCode.Update<T>(ErrorCodes.DatabaseError.DbUpdateException, out string entityName).Code,
                $"{errorDescriptionPrefix}: A Database-Exception occurred while updating the entity {entityName} in the database: {exception?.Message}",
                ErrorType.Failure);


        public static Error NullValue<T>(string currentOperation, string errorDescriptionPrefix, out string errorMessageReason)
        {
            var errorCode = ErrorCode.NullValue<T>(currentOperation, out string entityName);

            errorMessageReason = $"{entityName} cannot be null or default";

            // Create full error of ErrorType.Validation, with ErrorCode from above, and return it
            return new Error(
                errorCode.Code,
                $"{errorDescriptionPrefix}: {errorMessageReason}",
                ErrorType.Validation);

        }
        public static Error Validation(ErrorCode code, string description) =>
            new(code.Code, description, ErrorType.Validation);

        public static Error NotFound<T>(string errorDescriptionPrefix)
        {
            var errorCode = ErrorCode.Get<T>(ErrorCodes.GeneralError.NotFound, out string entityName);

            return new Error(errorCode.Code, $"{errorDescriptionPrefix}: {entityName} not found", ErrorType.NotFound);

        }

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