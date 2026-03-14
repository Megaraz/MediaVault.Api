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
            public const string DbException = "DbException";
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
        public ErrorCode(string operation, string nameOfEntity, string error)
        {
            Operation = operation;
            NameOfEntity = nameOfEntity;
            Error = error;

            Code = $"{Operation}.{NameOfEntity}.{Error}";
        }

        public string Operation { get; }
        public string NameOfEntity { get; }
        public string Error { get; }

        public string Code { get; }

        public override string ToString()
        {
            return $"{Operation}.{NameOfEntity}.{Error}";

        }

    }

    public sealed record Error(ErrorCode Code, string Description, ErrorType ErrorType)
    {
        public static Error Validation(ErrorCode code, string description) =>
            new(code, description, ErrorType.Validation);

        public static Error NotFound(ErrorCode code, string description) =>
            new(code, description, ErrorType.NotFound);

        public static Error Conflict(ErrorCode code, string description) =>
            new(code, description, ErrorType.Conflict);

        public static Error Unauthorized(ErrorCode code, string description) =>
            new(code, description, ErrorType.Unauthorized);

        public static Error Forbidden(ErrorCode code, string description) =>
            new(code, description, ErrorType.Forbidden);

        public static Error Failure(ErrorCode code, string description) =>
            new(code, description, ErrorType.Failure);

    }

}