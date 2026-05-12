using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class Result_Tests
    {
        // ── Success ──────────────────────────────────────────────────────────

        [Fact]
        public void Success_Should_Create_Valid_Success_Result()
        {
            var result = Result.Success();

            ResultTestAssertions.AssertValidSuccessResult(result);
        }

        // ── Failure ──────────────────────────────────────────────────────────

        [Fact]
        public void Failure_Should_Create_Valid_Failure_Result()
        {
            var errorContext = TestErrorContextFactory.Create(fieldName: "UserId");
            var error = Error.NotFound(errorContext);

            var result = Result.Failure(error, "Test error message");

            ResultTestAssertions.AssertValidFailureResult(result, error, "Test error message");
        }

        [Fact]
        public void Failure_Should_Use_Error_UserMessage_When_No_Message_Is_Provided()
        {
            var errorContext = TestErrorContextFactory.Create(fieldName: "UserId");
            var error = Error.NotFound(errorContext);

            var result = Result.Failure(error);

            ResultTestAssertions.AssertValidFailureResult(result, error, error.UserMessage);
        }

        [Fact]
        public void Failure_Should_Throw_When_PrimaryError_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => Result.Failure(null!));
        }

        [Fact]
        public void Failure_Should_Throw_When_PrimaryError_Is_None()
        {
            Assert.Throws<ArgumentException>(() => Result.Failure(Error.None));
        }

        // ── ValidationFailure ────────────────────────────────────────────────

        [Fact]
        public void ValidationFailure_Should_Create_Valid_Validation_Failure_Result()
        {
            var errorContext = TestErrorContextFactory.Create(fieldName: "Email");
            var validationError = ValidationError.InvalidFormat(errorContext, "mailname@adress.com");

            var result = Result.ValidationFailure([validationError]);

            ResultTestAssertions.AssertValidFailureResult(result, validationError, "Validation errors occurred, see validation errors for details.");
        }

        [Fact]
        public void ValidationFailure_Should_Use_Default_Message()
        {
            var errorContext = TestErrorContextFactory.Create(fieldName: "Email");
            var validationError = ValidationError.InvalidFormat(errorContext, "mailname@adress.com");

            var result = Result.ValidationFailure([validationError]);

            Assert.Equal("Validation errors occurred, see validation errors for details.", result.Message);
        }

        [Fact]
        public void ValidationFailure_Should_Use_Custom_Message()
        {
            var errorContext = TestErrorContextFactory.Create(fieldName: "Email");
            var validationError = ValidationError.InvalidFormat(errorContext, "mailname@adress.com");

            var result = Result.ValidationFailure([validationError], "Custom validation message");

            Assert.Equal("Custom validation message", result.Message);
        }

        [Fact]
        public void ValidationFailure_Should_Throw_When_Errors_Are_Null()
        {
            Assert.Throws<ArgumentNullException>(() => Result.ValidationFailure(null!));
        }

        [Fact]
        public void ValidationFailure_Should_Throw_When_Errors_Are_Empty()
        {
            Assert.Throws<ArgumentException>(() => Result.ValidationFailure(Array.Empty<ValidationError>()));
        }
    }
}
