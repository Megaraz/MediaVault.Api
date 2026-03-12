namespace Rasmus.SharedKernel.ResultPattern
{
    public abstract record Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        protected Result(bool isSuccess) => IsSuccess = isSuccess;
    }

    public abstract record Result<T> : Result
    {
        protected Result(bool isSuccess) : base(isSuccess) { }
    }

    public sealed record SuccessResult() : Result(true);
    public sealed record SuccessResult<T>(T Data) : Result<T>(true);

    public record ErrorResult(string Message, IReadOnlyCollection<Error> Errors)
        : Result(false), IErrorResult
    {
        public ErrorResult(string message) : this(message, Array.Empty<Error>()) { }
    }

    public record ErrorResult<T>(string Message, IReadOnlyCollection<Error> Errors)
        : Result<T>(false), IErrorResult
    {
        public ErrorResult(string message) : this(message, Array.Empty<Error>()) { }
    }

    public record ValidationErrorResult(string Message, IReadOnlyCollection<Error> Errors)
        : Result(false), IErrorResult
    {
        public ValidationErrorResult(string message) : this(message, Array.Empty<Error>()) { }
    }

    public record ValidationErrorResult<T>(string Message, IReadOnlyCollection<Error> Errors)
        : Result<T>(false), IErrorResult
    {
        public ValidationErrorResult(string message) : this(message, Array.Empty<Error>()) { }
    }
    public record Error(string? Code, string Details);

    public interface IErrorResult
    {
        string Message { get; }
        IReadOnlyCollection<Error> Errors { get; }
    }
}
