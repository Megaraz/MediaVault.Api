using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class ErrorContext_Tests
    {
        // ── Metadata fields ──────────────────────────────────────────────────
        // ErrorContext is metadata-only: it carries the technical context used
        // to generate error codes and diagnostic descriptions. It does not own
        // any description-formatting state.

        [Fact]
        public void ErrorContext_Should_Expose_All_Metadata_Fields()
        {
            var ctx = new ErrorContext(
                Layer: "Application",
                ServiceName: "UserService",
                MethodName: "CreateAsync",
                Operation: OperationType.Create,
                EntityName: "User");

            Assert.Equal("Application", ctx.Layer);
            Assert.Equal("UserService", ctx.ServiceName);
            Assert.Equal("CreateAsync", ctx.MethodName);
            Assert.Equal(OperationType.Create, ctx.Operation);
            Assert.Equal("User", ctx.EntityName);
        }

        // ── Optional FieldName ───────────────────────────────────────────────

        [Fact]
        public void FieldName_Should_Default_To_Null()
        {
            var ctx = new ErrorContext(
                Layer: "Application",
                ServiceName: "UserService",
                MethodName: "CreateAsync",
                Operation: OperationType.Create,
                EntityName: "User");

            Assert.Null(ctx.FieldName);
        }

        [Fact]
        public void FieldName_Can_Be_Set()
        {
            var ctx = new ErrorContext(
                Layer: "Application",
                ServiceName: "UserService",
                MethodName: "CreateAsync",
                Operation: OperationType.Create,
                EntityName: "User",
                FieldName: "Email");

            Assert.Equal("Email", ctx.FieldName);
        }

        // ── Record equality ──────────────────────────────────────────────────

        [Fact]
        public void ErrorContext_Should_Support_Value_Equality()
        {
            var ctx1 = new ErrorContext("Application", "UserService", "CreateAsync", OperationType.Create, "User");
            var ctx2 = new ErrorContext("Application", "UserService", "CreateAsync", OperationType.Create, "User");

            Assert.Equal(ctx1, ctx2);
        }

        [Fact]
        public void With_Expression_Should_Produce_New_Instance_With_Updated_FieldName()
        {
            var original = new ErrorContext("Application", "UserService", "CreateAsync", OperationType.Create, "User");
            var withField = original with { FieldName = "Email" };

            Assert.Null(original.FieldName);
            Assert.Equal("Email", withField.FieldName);
        }
    }
}
