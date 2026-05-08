using System;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class ValidatorExtensions_IsNullOrWhiteSpace_Tests
    {
        [Fact]
        public void Should_Return_True_And_Error_For_Null_String_With_FieldName()
        {
            string? value = null;
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNullOrWhiteSpace(value!, "Username", errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, fieldName: "Username", entityName: "User");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Return_True_And_Error_For_Invalid_String_With_FieldName(string value)
        {
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, fieldName: "Username", entityName: "User");
        }

        [Fact]
        public void Should_Return_False_And_No_Error_For_Valid_String_With_FieldName()
        {
            string value = "hello";
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var error);

            Assert.False(result);
            Assert.Null(error);
        }

        [Fact]
        public void Should_Return_True_And_Error_For_Null_String_Without_FieldName()
        {
            string? value = null;
            var errorContext = TestErrorContextFactory.Create(fieldName: "Username");

            var result = ValidatorExtensions.IsNullOrWhiteSpace(value!, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, fieldName: "Username", entityName: "User");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Return_True_And_Error_For_Invalid_String_Without_FieldName(string value)
        {
            var errorContext = TestErrorContextFactory.Create(fieldName: "Username");

            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, fieldName: "Username", entityName: "User");
        }

        [Fact]
        public void Should_Return_False_And_No_Error_For_Valid_String_Without_FieldName()
        {
            string value = "hello";
            var errorContext = TestErrorContextFactory.Create(fieldName: "Username");

            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var error);

            Assert.False(result);
            Assert.Null(error);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Fall_Back_To_Value_FieldName_When_FieldName_Argument_Is_Missing(string? fieldName)
        {
            var errorContext = TestErrorContextFactory.Create();

            var result = ValidatorExtensions.IsNullOrWhiteSpace(string.Empty, fieldName!, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, fieldName: "value", entityName: "User");
        }
    }
}
