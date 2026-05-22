using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class ErrorCode_Tests
    {
        // ── For() — copies values from ErrorContext ───────────────────────────

        [Fact]
        public void For_Should_Copy_Operation_From_ErrorContext()
        {
            var ctx = TestErrorContextFactory.Create(); // Operation = Create

            var errorCode = ErrorCode.For(ctx, ErrorReasonCode.ValidationRequired);

            Assert.Equal(OperationType.Create, errorCode.Operation);
        }

        [Fact]
        public void For_Should_Copy_EntityName_From_ErrorContext()
        {
            var ctx = TestErrorContextFactory.Create(); // EntityName = "User"

            var errorCode = ErrorCode.For(ctx, ErrorReasonCode.ValidationRequired);

            Assert.Equal("User", errorCode.NameOfEntity);
        }

        [Fact]
        public void For_Should_Set_Reason()
        {
            var ctx = TestErrorContextFactory.Create();

            var errorCode = ErrorCode.For(ctx, ErrorReasonCode.GeneralNotFound);

            Assert.Equal(ErrorReasonCode.GeneralNotFound, errorCode.Reason);
        }

        // ── Code — formatted as "Operation.Entity.Reason" ───────────────────

        [Fact]
        public void Code_Should_Be_Formatted_As_Operation_Entity_Reason()
        {
            var ctx = TestErrorContextFactory.Create(); // Create, User

            var errorCode = ErrorCode.For(ctx, ErrorReasonCode.ValidationRequired);

            Assert.Equal("Create.User.Required", errorCode.Code);
        }

        [Theory]
        [InlineData(ErrorReasonCode.GeneralNotFound,              "Create.User.NotFound")]
        [InlineData(ErrorReasonCode.DatabaseSaveChangesFailure,   "Create.User.DbSaveChangesFailure")]
        [InlineData(ErrorReasonCode.GeneralUnauthorized,          "Create.User.Unauthorized")]
        [InlineData(ErrorReasonCode.HttpInternalServerError,      "Create.User.InternalServerError")]
        public void Code_Should_Reflect_Different_Reasons(ErrorReasonCode reason, string expectedCode)
        {
            var ctx = TestErrorContextFactory.Create();

            var errorCode = ErrorCode.For(ctx, reason);

            Assert.Equal(expectedCode, errorCode.Code);
        }
    }
}
