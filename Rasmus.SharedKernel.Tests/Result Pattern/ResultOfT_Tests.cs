using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class ResultOfT_Tests
    {
        // ── Success ──────────────────────────────────────────────────────────

        [Fact]
        public void Generic_Success_Should_Return_Value()
        {
            var result = Result<int>.Success(42);

            Assert.Equal(42, result.Value);
            ResultTestAssertions.AssertValidSuccessResult(result);
        }

        [Fact]
        public void Generic_Success_Should_Throw_When_Value_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => Result<string>.Success(null!));
        }

        // ── Failure ──────────────────────────────────────────────────────────

        [Fact]
        public void Generic_Failure_Should_Throw_When_Accessing_Value()
        {
            var errorContext = TestErrorContextFactory.Create(fieldName: "UserId");
            var error = Error.NotFound(errorContext);

            var result = Result<int>.Failure(error, "Test error message");

            Assert.Throws<InvalidOperationException>(() => { var value = result.Value; });
            ResultTestAssertions.AssertValidFailureResult(result, error, "Test error message");
        }

        [Fact]
        public void Generic_Failure_Should_Throw_When_PrimaryError_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => Result<int>.Failure(null!));
        }

        [Fact]
        public void Generic_Failure_Should_Throw_When_PrimaryError_Is_None()
        {
            Assert.Throws<ArgumentException>(() => Result<int>.Failure(Error.None));
        }

        // ── ValidationFailure ────────────────────────────────────────────────

        [Fact]
        public void Generic_ValidationFailure_Should_Throw_When_Accessing_Value()
        {
            var errorContext = TestErrorContextFactory.Create(fieldName: "Email");
            var validationError = ValidationError.InvalidFormat(errorContext, "mailname@adress.com");

            var result = Result<string>.ValidationFailure([validationError]);

            Assert.Throws<InvalidOperationException>(() => { var value = result.Value; });
            ResultTestAssertions.AssertValidFailureResult(result, validationError, "Validation errors occurred, see validation errors for details.");
        }

        [Fact]
        public void Generic_ValidationFailure_Should_Throw_When_Errors_Are_Null()
        {
            Assert.Throws<ArgumentNullException>(() => Result<int>.ValidationFailure(null!));
        }

        [Fact]
        public void Generic_ValidationFailure_Should_Throw_When_Errors_Are_Empty()
        {
            Assert.Throws<ArgumentException>(() => Result<int>.ValidationFailure(Array.Empty<ValidationError>()));
        }
    }
}
