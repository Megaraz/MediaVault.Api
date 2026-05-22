using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern
{
    public class ErrorContext_Tests
    {
        // ── DescriptionPrefix — contains all context fields ──────────────────

        [Fact]
        public void DescriptionPrefix_Should_Include_Operation_And_EntityName()
        {
            var ctx = new ErrorContext(
                Layer: "Application",
                ServiceName: "UserService",
                MethodName: "CreateAsync",
                Operation: OperationType.Create,
                EntityName: "User");

            Assert.Contains("Create", ctx.DescriptionPrefix);
            Assert.Contains("User", ctx.DescriptionPrefix);
        }

        [Fact]
        public void DescriptionPrefix_Should_Include_Layer_ServiceName_And_MethodName()
        {
            var ctx = new ErrorContext(
                Layer: "Infrastructure",
                ServiceName: "OrderRepo",
                MethodName: "GetByIdAsync",
                Operation: OperationType.Get,
                EntityName: "Order");

            Assert.Contains("Infrastructure", ctx.DescriptionPrefix);
            Assert.Contains("OrderRepo", ctx.DescriptionPrefix);
            Assert.Contains("GetByIdAsync", ctx.DescriptionPrefix);
        }

        // ── FullDescription — suffix fallback ────────────────────────────────

        [Fact]
        public void FullDescription_Should_Contain_Unknown_Or_Unspecified_When_DescriptionSuffix_Is_Null()
        {
            var ctx = new ErrorContext(
                Layer: "Application",
                ServiceName: "UserService",
                MethodName: "CreateAsync",
                Operation: OperationType.Create,
                EntityName: "User");

            Assert.Contains("Unknown or unspecified", ctx.FullDescription);
        }

        [Fact]
        public void FullDescription_Should_Contain_Custom_Suffix_When_DescriptionSuffix_Is_Set()
        {
            var ctx = new ErrorContext(
                Layer: "Application",
                ServiceName: "UserService",
                MethodName: "CreateAsync",
                Operation: OperationType.Create,
                EntityName: "User")
            {
                DescriptionSuffix = "Database timed out."
            };

            Assert.Contains("Database timed out.", ctx.FullDescription);
        }

        [Fact]
        public void FullDescription_Should_Not_Contain_Unknown_Or_Unspecified_When_DescriptionSuffix_Is_Set()
        {
            var ctx = new ErrorContext(
                Layer: "Application",
                ServiceName: "UserService",
                MethodName: "CreateAsync",
                Operation: OperationType.Create,
                EntityName: "User")
            {
                DescriptionSuffix = "Something specific."
            };

            Assert.DoesNotContain("Unknown or unspecified", ctx.FullDescription);
        }

        // ── DescriptionSuffix — default ──────────────────────────────────────

        [Fact]
        public void DescriptionSuffix_Should_Default_To_Null()
        {
            var ctx = new ErrorContext(
                Layer: "Application",
                ServiceName: "UserService",
                MethodName: "CreateAsync",
                Operation: OperationType.Create,
                EntityName: "User");

            Assert.Null(ctx.DescriptionSuffix);
        }
    }
}
