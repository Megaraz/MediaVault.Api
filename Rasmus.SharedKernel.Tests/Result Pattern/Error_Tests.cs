using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class Error_Tests
    {
        // ── Unauthorized ─────────────────────────────────────────────────────

        [Fact]
        public void Unauthorized_Without_FieldName_Should_Not_Include_Field_In_UserMessage()
        {
            var errorContext = TestErrorContextFactory.Create(fieldName: null);

            var error = Error.Unauthorized(errorContext);

            Assert.Equal(ErrorType.Unauthorized, error.Type);
            Assert.Equal("Unauthorized access", error.UserMessage);
        }

        [Fact]
        public void Unauthorized_With_FieldName_Should_Include_Field_In_UserMessage()
        {
            var errorContext = TestErrorContextFactory.Create(fieldName: "Email");

            var error = Error.Unauthorized(errorContext);

            Assert.Equal(ErrorType.Unauthorized, error.Type);
            Assert.Equal("Unauthorized access to Email", error.UserMessage);
        }

        // ── Failure ──────────────────────────────────────────────────────────

        [Fact]
        public void Failure_Without_DescriptionSuffix_Should_Use_Default_UserMessage()
        {
            var errorContext = TestErrorContextFactory.Create();

            var error = Error.Failure(errorContext);

            Assert.Equal(ErrorType.Failure, error.Type);
            Assert.Equal("An unexpected failure occurred while processing User.", error.UserMessage);
        }

        [Fact]
        public void Failure_With_DescriptionSuffix_Should_Use_Custom_UserMessage()
        {
            var errorContext = TestErrorContextFactory.Create();
            const string customSuffix = "Something specific went wrong.";

            var error = Error.Failure(errorContext, descriptionSuffix: customSuffix);

            Assert.Equal(ErrorType.Failure, error.Type);
            Assert.Equal(customSuffix, error.UserMessage);
        }

        [Fact]
        public void Failure_With_Exception_Should_Attach_Exception()
        {
            var errorContext = TestErrorContextFactory.Create();
            var exception = new InvalidOperationException("boom");

            var error = Error.Failure(errorContext, exception: exception);

            Assert.Equal(exception, error.Exception);
        }
    }
}
