using System;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class ValidatorExtensions_DoesNotMatch_Tests
    {
        [Fact]
        public void Should_Return_True_And_Error_When_Strings_Do_Not_Match()
        {
            string value1 = "password123";
            string value2 = "password456";
            var errorContext = TestErrorContextFactory.Create(fieldName: "Password", confirmFieldName: "ConfirmPassword");

            var result = ValidatorExtensions.DoesNotMatch(value1, value2, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsNonMatching(error, "Password", "ConfirmPassword", "User");
        }

        [Fact]
        public void Should_Return_True_And_Error_When_Case_Differs()
        {
            string value1 = "Password";
            string value2 = "password";
            var errorContext = TestErrorContextFactory.Create(fieldName: "Password", confirmFieldName: "ConfirmPassword");

            var result = ValidatorExtensions.DoesNotMatch(value1, value2, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsNonMatching(error, "Password", "ConfirmPassword", "User");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Return_True_And_Required_Error_When_Value1_Is_Null_Or_Whitespace(string? value1)
        {
            string value2 = "password123";
            var errorContext = TestErrorContextFactory.Create(fieldName: "Password", confirmFieldName: "ConfirmPassword");

            var result = ValidatorExtensions.DoesNotMatch(value1!, value2, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, fieldName: "Password", entityName: "User");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Return_True_And_Required_Error_When_Value2_Is_Null_Or_Whitespace(string? value2)
        {
            string value1 = "password123";
            var errorContext = TestErrorContextFactory.Create(fieldName: "Password", confirmFieldName: "ConfirmPassword");

            var result = ValidatorExtensions.DoesNotMatch(value1, value2!, errorContext, out var error);

            Assert.True(result);
            ValidationErrorAssert.IsRequired(error, fieldName: "ConfirmPassword", entityName: "User");
        }

        [Fact]
        public void Should_Return_False_And_No_Error_When_Strings_Match()
        {
            string value1 = "password123";
            string value2 = "password123";
            var errorContext = TestErrorContextFactory.Create(fieldName: "Password", confirmFieldName: "ConfirmPassword");

            var result = ValidatorExtensions.DoesNotMatch(value1, value2, errorContext, out var error);

            Assert.False(result);
            Assert.Null(error);
        }
    }
}
