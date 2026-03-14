namespace Rasmus.SharedKernel.ResultPattern
{
    // public abstract record Result
    // {
    //     public bool IsSuccess { get; }
    //     public bool IsFailure => !IsSuccess;

    //     protected Result(bool isSuccess) => IsSuccess = isSuccess;
    // }

    // public abstract record Result<T> : Result
    // {
    //     protected Result(bool isSuccess) : base(isSuccess) { }
    // }

    // public sealed record SuccessResult() : Result(true);
    // public sealed record SuccessResult<T>(T Data) : Result<T>(true);

    // public record ErrorResult(string Message, IReadOnlyCollection<Error> Errors)
    //     : Result(false), IErrorResult
    // {
    //     public ErrorResult(string message) : this(message, Array.Empty<Error>()) { }
    // }

    // public record ErrorResult<T>(string Message, IReadOnlyCollection<Error> Errors)
    //     : Result<T>(false), IErrorResult
    // {
    //     public ErrorResult(string message) : this(message, Array.Empty<Error>()) { }
    // }

    // public record ValidationErrorResult(string Message, IReadOnlyCollection<Error> Errors)
    //     : Result(false), IErrorResult
    // {
    //     public ValidationErrorResult(string message) : this(message, Array.Empty<Error>()) { }
    // }

    // public record ValidationErrorResult<T>(string Message, IReadOnlyCollection<Error> Errors)
    //     : Result<T>(false), IErrorResult
    // {
    //     public ValidationErrorResult(string message) : this(message, Array.Empty<Error>()) { }
    // }
    // public record Error(string? Code, string Details);

    // public interface IErrorResult
    // {
    //     string Message { get; }
    //     IReadOnlyCollection<Error> Errors { get; }
    // }
    // public class ValidationErrorCode
    // {
    //     public string? NameOfEntity { get; set; }
    //     public ActionType ActionType { get; set; }

    //     public ValidationErrorCode(string nameOfEntity, ActionType action)
    //     {
    //         NameOfEntity = nameOfEntity;
    //         ActionType = action;
    //     }

    //     public override string ToString()
    //     {
    //         return $"{NameOfEntity}.{ActionType.Action}";
    //     }


    // }

    // public abstract record BaseErrorType
    // {
    //     public string Type { get; private set; }
    //     protected BaseErrorType(string type) => Type = type;

    //     public const string ValidationError = "ValidationError";
    //     public const string NotFoundError = "NotFoundError";
    //     public const string UnauthorizedError = "UnauthorizedError";
    //     public const string ForbiddenError = "ForbiddenError";
    //     public const string ConflictError = "ConflictError";
    //     public const string InternalServerError = "InternalServerError";
    // }

    // public record ValidationError : BaseErrorType
    // {
    //     public string ValidationType { get; set; }
    //     public ValidationError(string validationType) : base(ValidationError)
    //     {
    //         ValidationType = validationType;

    //     }

    //     public const string InvalidInput = "InvalidInput";
    //     public const string NullValue = "NullValue";
    //     public const string OutOfRange = "OutOfRange";
    //     public const string FormatError = "FormatError";
    //     public const string RequiredFieldMissing = "RequiredFieldMissing";
    // }

    // public record InvalidInput : ValidationError
    // {
    //     public InvalidInput() : base(InvalidInput) { }
    // }

    // public sealed record NotFoundError : BaseErrorType
    // {
    //     public NotFoundError() : base(NotFoundError) { }
    // }

    // public abstract record ActionType
    // {
    //     public string Action { get; private set; }
    //     protected ActionType(string action) => Action = action;


    //     public const string Create = "Create";
    //     public const string Read = "Read";
    //     public const string Update = "Update";
    //     public const string Delete = "Delete";
    // }

    // public sealed record CreateAction : ActionType
    // {
    //     public CreateAction() : base(Create) { }
    // }
    // public sealed record ReadAction : ActionType
    // {
    //     public ReadAction() : base(Read) { }
    // }
    // public sealed record UpdateAction : ActionType
    // {
    //     public UpdateAction() : base(Update) { }
    // }
    // public sealed record DeleteAction : ActionType
    // {
    //     public DeleteAction() : base(Delete) { }
    // }

}
