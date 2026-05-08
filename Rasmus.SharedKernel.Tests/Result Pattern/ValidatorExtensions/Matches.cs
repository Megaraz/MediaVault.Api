using System;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class ValidatorExtensions_Matches_Tests
    {
        [Fact]
        public void Should_Return_False_And_Error_When_Strings_Do_Not_Match()
        {
            string value1 = "password123";
            string value2 = "password456";
            var errorContext = TestErrorContextFactory.Create(fieldName: "Password", confirmFieldName: "ConfirmPassword");

            var result = ValidatorExtensions.DoesNotMatch(value1, value2, errorContext, out var error);

            Assert.False(result);
            ValidationErrorAssert.IsNonMatching(error, "Password", "ConfirmPassword", "User");
        }

        [Fact]
        public void Should_Return_False_And_Error_When_Case_Differs()
        {
            string value1 = "Password";
            string value2 = "password";
            var errorContext = TestErrorContextFactory.Create(fieldName: "Password", confirmFieldName: "ConfirmPassword");

            var result = ValidatorExtensions.DoesNotMatch(value1, value2, errorContext, out var error);

            Assert.False(result);
            ValidationErrorAssert.IsNonMatching(error, "Password", "ConfirmPassword", "User");
        }

        [Fact]
        public void Should_Return_False_And_Error_When_One_Value_Is_Null()
        {
            string value1 = "password123";
            string value2 = null!;
            var errorContext = TestErrorContextFactory.Create(fieldName: "Password", confirmFieldName: "ConfirmPassword");

            var result = ValidatorExtensions.DoesNotMatch(value1, value2, errorContext, out var error);

            Assert.False(result);
            ValidationErrorAssert.IsNonMatching(error, "Password", "ConfirmPassword", "User");
        }

        [Fact]
        public void Should_Return_True_And_No_Error_When_Both_Values_Are_Null()
        {
            string value1 = null!;
            string value2 = null!;
            var errorContext = TestErrorContextFactory.Create(fieldName: "Password", confirmFieldName: "ConfirmPassword");

            var result = ValidatorExtensions.DoesNotMatch(value1, value2, errorContext, out var error);

            Assert.True(result);
            Assert.Null(error);
        }

        [Fact]
        public void Should_Return_True_And_No_Error_When_Strings_Match()
        {
            string value1 = "password123";
            string value2 = "password123";
            var errorContext = TestErrorContextFactory.Create(fieldName: "Password", confirmFieldName: "ConfirmPassword");

            var result = ValidatorExtensions.DoesNotMatch(value1, value2, errorContext, out var error);

            Assert.True(result);
            Assert.Null(error);
        }

        [Fact]
        public void Should_Return_True_And_No_Error_When_Both_Are_Empty()
        {
            string value1 = string.Empty;
            string value2 = string.Empty;
            var errorContext = TestErrorContextFactory.Create(fieldName: "Password", confirmFieldName: "ConfirmPassword");

            var result = ValidatorExtensions.DoesNotMatch(value1, value2, errorContext, out var error);

            Assert.True(result);
            Assert.Null(error);
        }
    }
}
