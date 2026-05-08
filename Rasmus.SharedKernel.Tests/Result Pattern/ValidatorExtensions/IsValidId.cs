using System;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class ValidatorExtensions_IsValidId_Tests
    {
        [Fact]
        public void Should_Return_True_And_Error_For_Null_Integer()
        {
            int? id = null;
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNotValidId(id, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, entityName: "User");
            Assert.Contains("id", error.Description, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Should_Return_True_And_Error_For_Invalid_Integer(int id)
        {
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNotValidId(id, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, entityName: "User");
            Assert.Contains("id", error.Description, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Should_Return_False_And_No_Error_For_Valid_Integer()
        {
            int id = 123;
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNotValidId(id, errorContext, out var error);

            Assert.False(result);
            Assert.Null(error);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Return_True_And_Error_For_Invalid_String(string id)
        {
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNotValidId(id, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, entityName: "User");
            Assert.Contains("id", error.Description, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Should_Return_True_And_Error_For_Null_String()
        {
            string? id = null;
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNotValidId(id, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, entityName: "User");
            Assert.Contains("id", error.Description, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Should_Return_False_And_No_Error_For_Valid_String()
        {
            string id = "valid-id";
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNotValidId(id, errorContext, out var error);

            Assert.False(result);
            Assert.Null(error);
        }

        [Fact]
        public void Should_Return_True_And_Error_For_Empty_Guid()
        {
            Guid id = Guid.Empty;
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNotValidId(id, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, entityName: "User");
            Assert.Contains("id", error.Description, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Should_Return_True_And_Error_For_Null_Guid()
        {
            Guid? id = null;
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNotValidId(id, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, entityName: "User");
            Assert.Contains("id", error.Description, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Should_Return_False_And_No_Error_For_Valid_Guid()
        {
            Guid id = Guid.NewGuid();
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNotValidId(id, errorContext, out var error);

            Assert.False(result);
            Assert.Null(error);
        }

        [Fact]
        public void Should_Use_FieldName_From_ErrorContext_When_Provided()
        {
            var errorContext = TestErrorContextFactory.Create(fieldName: "UserId");

            var result = ValidatorExtensions.IsNotValidId(Guid.Empty, errorContext, out var error);
            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, fieldName: "UserId", entityName: "User");
        }
    }
}
