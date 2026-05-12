namespace Rasmus.SharedKernel.ResultPattern
{

    /// <summary>
    /// Represents the outcome of an operation without a return value.
    /// </summary>
    public class Result
    {
        protected const string DefaultValidationFailureMessage =
            "Validation errors occurred, see validation errors for details.";


        /// <summary>
        /// Gets whether the operation completed successfully.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets the message for the result.
        /// </summary>
        public string Message { get; }


        /// <summary>
        /// Gets the validation errors for a validation failure.
        /// </summary>
        public IReadOnlyCollection<ValidationError> ValidationErrors { get; }

        /// <summary>
        /// Gets the main error for a failed result.
        /// </summary>
        public Error PrimaryError { get; }


        #region **CONSTRUCTORS TO ENFORCE THE USE OF FACTORY METHODS**

        // Main constructor, for internal use only, with validation logic to ensure consistency of the Result state
        /// <summary>
        /// Initializes a new result with the provided state.
        /// </summary>
        /// <param name="isSuccess">Whether the result is successful.</param>
        /// <param name="message">The result message.</param>
        /// <param name="validationErrors">The validation errors for the result.</param>
        /// <param name="primaryError">The main error for the result.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown if the result state is inconsistent.</exception>

        internal Result(
            bool isSuccess,
            string message,
            IReadOnlyCollection<ValidationError> validationErrors,
            Error primaryError)
        {
            // **|| GUARD CLAUSES TO ENSURE CONSISTENCY OF THE RESULT STATE ||**
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(validationErrors);
            ArgumentNullException.ThrowIfNull(primaryError);


            // SUCCESS GUARD CLAUSES 
            if (isSuccess)
            {
                if (primaryError.Type != ErrorType.None)
                    throw new ArgumentException(
                        $"Success result cannot contain errors. {nameof(primaryError)}");

                if (validationErrors.Count > 0)
                    throw new ArgumentException(
                        $"Success result cannot contain validation errors. {nameof(validationErrors)}");
            }

            // FAILURE GUARD CLAUSES
            if (!isSuccess)
            {
                if (primaryError.Type == ErrorType.None)
                    throw new ArgumentException(
                        $"Failure result must contain a primary error. {nameof(primaryError)}");


                if (primaryError.Type == ErrorType.Validation)
                {
                    if (primaryError is not ValidationError validationPrimary)
                        throw new ArgumentException(
                            $"Validation failure result must have an error of type ValidationError. {nameof(primaryError)}");

                    if (validationErrors.Count == 0)
                        throw new ArgumentException(
                            $"Validation failure result must contain a collection of validation errors. {nameof(validationErrors)}");

                    if (!validationErrors.Contains(validationPrimary))
                        throw new ArgumentException(
                            $"Validation errors must contain the primary validation error. {nameof(validationErrors)}");
                }
                else
                {
                    if (validationErrors.Count > 0)
                        throw new ArgumentException(
                            $"Non-validation failure result cannot contain validation errors. {nameof(validationErrors)}");

                }
            }

            IsSuccess = isSuccess;
            Message = message;
            //ValidationErrors = validationErrors;
            ValidationErrors = validationErrors.ToArray();
            PrimaryError = primaryError;
        }

        // Success Result convenience constructor, for internal use only
        /// <summary>
        /// Initializes a successful result.
        /// </summary>
        protected Result() : this(true, string.Empty, Array.Empty<ValidationError>(), Error.None)
        {
        }

        #endregion

        #region **PUBLIC FACTORY METHODS FOR CREATING RESULT INSTANCES**

        // Main Success factory method, for creating a successful Result without errors
        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <returns>A successful <see cref="Result"/>.</returns>
        public static Result Success() => new();

        // Validation Failure factory method, for creating a failed Validation Result with a collection of validation-errors and a message
        /// <summary>
        /// Creates a validation failure result.
        /// </summary>
        /// <param name="validationErrors">The validation errors to include.</param>
        /// <param name="message">The result message.</param>
        /// <returns>A failed <see cref="Result"/> with validation errors.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationErrors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="validationErrors"/> is empty.</exception>
        public static Result ValidationFailure(
            IEnumerable<ValidationError> validationErrors,
            string? message = null)
        {
            ArgumentNullException.ThrowIfNull(validationErrors);

            var errors = validationErrors.ToList();

            if (errors.Count == 0)
                throw new ArgumentException(
                    "Validation failure must contain at least one validation error.",
                    nameof(validationErrors));

            return new Result(
                isSuccess: false,
                message: string.IsNullOrWhiteSpace(message)
                    ? DefaultValidationFailureMessage
                    : message,
                validationErrors: errors,
                primaryError: errors[0]);
        }


        // Convenience Failure factory method, for creating a failed Result with a single error and a message
        /// <summary>
        /// Creates a failure result using the error's <see cref="Error.UserMessage"/> as the result message.
        /// </summary>
        /// <param name="primaryError">The main error for the failure.</param>
        /// <returns>A failed <see cref="Result"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="primaryError"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="primaryError"/> does not represent a valid non-validation failure.</exception>
        public static Result Failure(Error primaryError)
        {
            ArgumentNullException.ThrowIfNull(primaryError);

            return Failure(primaryError, primaryError.UserMessage);
        }

        // Convenience Failure factory method, for creating a failed Result with a single error and a message
        /// <summary>
        /// Creates a failure result.
        /// </summary>
        /// <param name="primaryError">The main error for the failure.</param>
        /// <param name="message">The result message.</param>
        /// <returns>A failed <see cref="Result"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="primaryError"/> or <paramref name="message"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="primaryError"/> does not represent a valid non-validation failure.</exception>
        public static Result Failure(Error primaryError, string message)
        {
            ArgumentNullException.ThrowIfNull(primaryError);
            ArgumentNullException.ThrowIfNull(message);

            return new Result(
                isSuccess: false,
                message: message,
                validationErrors: Array.Empty<ValidationError>(),
                primaryError: primaryError);
        }

        #endregion
    }

    /// <summary>
    /// Represents the outcome of an operation with a return value.
    /// </summary>
    /// <typeparam name="TValue">The value type returned on success.</typeparam>
    public sealed class Result<TValue> : Result
    {
        private readonly TValue? _value;

        // Guard Property 
        /// <summary>
        /// Gets the value of a successful result.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the result is a failure.</exception>
        public TValue Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access value of a failed result.");


        #region **CONSTRUCTORS TO ENFORCE THE USE OF FACTORY METHODS**

        // Success Result constructor, for internal use only, with validation logic (on the base-class) to ensure consistency of the Result state
        /// <summary>
        /// Initializes a successful result with a value.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <exception cref="ArgumentNullException">Thrown if the value is null.</exception>
        private Result(TValue value) : base()
        {
            ArgumentNullException.ThrowIfNull(value);
            _value = value;
        }

        // Failure Result constructor, for internal use only, with validation logic (on the base-class) to ensure consistency of the Result state
        /// <summary>
        /// Initializes a failed result.
        /// </summary>
        /// <param name="message">The result message.</param>
        /// <param name="validationErrors">The validation errors for the result.</param>
        /// <param name="primaryError">The main error for the result.</param>
        internal Result(
            string message,
            IReadOnlyCollection<ValidationError> validationErrors,
            Error primaryError) : base(false, message, validationErrors, primaryError)
        {
            _value = default;
        }
        #endregion

        #region **PUBLIC FACTORY METHODS FOR CREATING RESULT INSTANCES**

        // Main Success factory method, for creating a successful Result without errors
        /// <summary>
        /// Creates a successful result with a value.
        /// </summary>
        /// <param name="value">The value to return.</param>
        /// <returns>A successful <see cref="Result{TValue}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the value is null.</exception>
        public static Result<TValue> Success(TValue value) => new(value);


        // Validation Failure factory method, for creating a failed Validation Result with a collection of validation-errors and a message
        /// <summary>
        /// Creates a validation failure result.
        /// </summary>
        /// <param name="validationErrors">The validation errors to include.</param>
        /// <param name="message">The result message.</param>
        /// <returns>A failed <see cref="Result{TValue}"/> with validation errors.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationErrors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="validationErrors"/> is empty.</exception>
        public new static Result<TValue> ValidationFailure(
            IEnumerable<ValidationError> validationErrors,
            string? message = null)
        {
            ArgumentNullException.ThrowIfNull(validationErrors);

            var errors = validationErrors.ToList();

            if (errors.Count == 0)
                throw new ArgumentException(
                    "Validation failure must contain at least one validation error.",
                    nameof(validationErrors));

            return new Result<TValue>(
                message: string.IsNullOrWhiteSpace(message)
                    ? DefaultValidationFailureMessage
                    : message,
                validationErrors: errors,
                primaryError: errors[0]);
        }


        // Failure factory method, for creating a failed Result using the error's UserMessage
        /// <summary>
        /// Creates a failure result using the error's <see cref="Error.UserMessage"/> as the result message.
        /// </summary>
        /// <param name="primaryError">The main error for the failure.</param>
        /// <returns>A failed <see cref="Result{TValue}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="primaryError"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="primaryError"/> does not represent a valid non-validation failure.</exception>
        public new static Result<TValue> Failure(Error primaryError)
        {
            ArgumentNullException.ThrowIfNull(primaryError);

            return Failure(primaryError, primaryError.UserMessage);
        }

        // Failure factory method, for creating a failed Result with a single error and a message
        /// <summary>
        /// Creates a failure result.
        /// </summary>
        /// <param name="primaryError">The main error for the failure.</param>
        /// <param name="message">The result message.</param>
        /// <returns>A failed <see cref="Result{TValue}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="primaryError"/> or <paramref name="message"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="primaryError"/> does not represent a valid non-validation failure.</exception>
        public new static Result<TValue> Failure(Error primaryError, string message)
        {
            ArgumentNullException.ThrowIfNull(primaryError);
            ArgumentNullException.ThrowIfNull(message);

            return new Result<TValue>(
                message: message,
                validationErrors: Array.Empty<ValidationError>(),
                primaryError: primaryError);
        }


        // Implicit conversions for cleaner syntax
        /// <summary>
        /// Converts a value to a successful result.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        public static implicit operator Result<TValue>(TValue value) => Success(value);
        //public static implicit operator Result<TValue>(Error error) => Failure(error);

        #endregion

    }

}