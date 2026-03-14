namespace Rasmus.SharedKernel.ResultPattern
{

    public record Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Message { get; }
        public IReadOnlyCollection<Error> Errors { get; }

        // **PROTECTED CONSTRUCTORS TO ENFORCE THE USE OF FACTORY METHODS**
        // Main constructor, for internal use only, with validation logic to ensure consistency of the Result state
        protected Result(bool isSuccess, string message, IReadOnlyCollection<Error> errors)
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(errors);

            if (isSuccess && errors.Count > 0)
                throw new ArgumentException("Success result cannot contain errors.", nameof(errors));

            if (!isSuccess && errors.Count == 0)
                throw new ArgumentException("Failure result must contain at least one error.", nameof(errors));

            IsSuccess = isSuccess;
            Message = message;
            Errors = errors;
        }

        // Success Result convenience constructor, for internal use only
        protected Result(bool isSuccess) : this(isSuccess, string.Empty, Array.Empty<Error>())
        {
        }

        // **PUBLIC FACTORY METHODS FOR CREATING RESULT INSTANCES**

        // Main Success factory method, for creating a successful Result without errors
        public static Result Success() => new(true);

        // Main Failure factory method, for creating a failed Result with a collection of errors and a message
        public static Result Failure(IReadOnlyCollection<Error> errors, string message) =>
            new(false, message, errors);

        // Convenience Failure factory method, for creating a failed Result with a single error and a message
        public static Result Failure(Error error, string message) =>
            new(false, message, new[] { error });
    }

    public record Result<TValue> : Result
    {
        private readonly TValue? _value;

        // Guard Property 
        public TValue Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access value of a failed result.");


        // **PROTECTED CONSTRUCTORS TO ENFORCE THE USE OF FACTORY METHODS*

        // Success Result constructor, for internal use only, with validation logic (on the base-class) to ensure consistency of the Result state
        private Result(TValue value) : base(true)
        {
            _value = value;
        }

        // Failure Result constructor, for internal use only, with validation logic (on the base-class) to ensure consistency of the Result state
        private Result(IReadOnlyCollection<Error> errors, string message) : base(false, message, errors)
        {
            _value = default;
        }


        // **PUBLIC FACTORY METHODS FOR CREATING RESULT INSTANCES**

        // Main Success factory method, for creating a successful Result without errors
        public static Result<TValue> Success(TValue value) => new(value);

        // Main Failure factory method, for creating a failed Result with a collection of errors and a message
        public new static Result<TValue> Failure(IReadOnlyCollection<Error> errors, string message) => 
            new(errors, message);

        // Convenience Failure factory method, for creating a failed Result with a single error and a message
        public new static Result<TValue> Failure(Error error, string message) => 
            new(new[] { error }, message);

        // Implicit conversions for cleaner syntax
        public static implicit operator Result<TValue>(TValue value) => Success(value);
        //public static implicit operator Result<TValue>(Error error) => Failure(error);

    }

}