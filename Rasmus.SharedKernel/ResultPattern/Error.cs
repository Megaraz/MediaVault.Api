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
        Forbidden = 6                                            ,
        Database = 7,
        HttpError = 8

    }

    public record Error(string Code, string Description, ErrorType Type, string UserMessage = "", Exception? exception = null)
    {

        public static readonly Error None = new Error(string.Empty, string.Empty, ErrorType.None);

        public override string ToString()
        {
            return $"Error Code: {Code}{Environment.NewLine}Description: {Description}";
        }

        public static Error NotFound(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.GeneralNotFound);

            string defaultDescriptionSuffix = $"{errorContext.EntityName} not found";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new Error(errorCode.Code, formattedErrorDescription, ErrorType.NotFound, defaultDescriptionSuffix);
        }

        public static Error Conflict(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.GeneralConflict);

            string defaultDescriptionSuffix = $"Unique {errorContext.EntityName} constraint violated.";

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new Error(errorCode.Code, formattedErrorDescription, ErrorType.Conflict, defaultDescriptionSuffix);
        }

        public static Error Unauthorized(ErrorContext errorContext)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.GeneralUnauthorized);

            string defaultDescriptionSuffix = $"Unauthorized access" + (string.IsNullOrWhiteSpace(errorContext.FieldName) ? "" : $" to {errorContext.FieldName}");

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new Error(errorCode.Code, formattedErrorDescription, ErrorType.Unauthorized, defaultDescriptionSuffix);
        }

        public static Error Failure(ErrorContext errorContext, string? descriptionSuffix = null, Exception? exception = null)
        {
            var errorCode = ErrorCode.For(errorContext, ErrorReasonCode.GeneralFailure);

            string defaultDescriptionSuffix = string.IsNullOrWhiteSpace(descriptionSuffix)
                ? $"An unexpected failure occurred while processing {errorContext.EntityName}."
                : descriptionSuffix;

            string formattedErrorDescription = FormatDescription(errorContext, defaultDescriptionSuffix);

            return new Error(errorCode.Code, formattedErrorDescription, ErrorType.Failure, defaultDescriptionSuffix, exception);
        }

        protected static string FormatDescription(ErrorContext errorContext, string defaultDescriptionSuffix)
        {
            return $"{errorContext.DescriptionPrefix}: {(string.IsNullOrWhiteSpace(errorContext.DescriptionSuffix) ? defaultDescriptionSuffix : errorContext.DescriptionSuffix)}";
        }

    }

}