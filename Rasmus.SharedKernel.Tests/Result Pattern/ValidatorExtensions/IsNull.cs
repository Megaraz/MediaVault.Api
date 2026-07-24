using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class ValidatorExtensions_IsNull_Tests
    {
        [Fact]
        public void Should_Return_True_And_Error_For_Null_Object()
        {
            object? value = null;
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNull(value, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, entityName: "User");
        }

        [Fact]
        public void Should_Return_True_And_Error_For_Null_String()
        {
            string? value = null;
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNull(value, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, entityName: "User");
        }

        [Fact]
        public void Should_Return_False_And_No_Error_For_NonNull_Object()
        {
            var value = new object();
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNull(value, errorContext, out var error);

            Assert.False(result);
            Assert.Null(error);
        }

        [Fact]
        public void Should_Return_False_And_No_Error_For_NonNull_String()
        {
            string value = "hello";
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNull(value, errorContext, out var error);

            Assert.False(result);
            Assert.Null(error);
        }

        [Fact]
        public void Should_Return_False_And_No_Error_For_NonNull_Integer()
        {
            int? value = 42;
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNull(value, errorContext, out var error);

            Assert.False(result);
            Assert.Null(error);
        }
    }
}
