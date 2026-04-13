using System;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class Matches
    {
        [Fact]
        public void Should_Return_False_And_Error_When_Strings_Do_Not_Match()
        {
            // Arrange
            string value1 = "password123";
            string value2 = "password456";
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            var result = ValidatorExtensions.Matches(value1, value2, errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.NonMatchingValues, error.ValidationErrorType);
        }

        [Fact]
        public void Should_Return_False_And_Error_When_Case_Differs()
        {
            // Arrange
            string value1 = "Password";
            string value2 = "password";
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            var result = ValidatorExtensions.Matches(value1, value2, errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.NonMatchingValues, error.ValidationErrorType);
        }

        [Fact]
        public void Should_Return_False_And_Error_When_One_Value_Is_Null()
        {
            // Arrange
            string value1 = "password123";
            string value2 = null!;
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            var result = ValidatorExtensions.Matches(value1, value2, errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.NotNull(error);
            Assert.Equal(ValidationErrorType.NonMatchingValues, error.ValidationErrorType);
        }

        [Fact]
        public void Should_Return_True_And_No_Error_When_Both_Values_Are_Null()
        {
            // Arrange
            string value1 = null!;
            string value2 = null!;
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            var result = ValidatorExtensions.Matches(value1, value2, errorContext, out var error);

            // Assert
            Assert.True(result);
            Assert.Null(error);
        }

        [Fact]
        public void Should_Return_True_And_No_Error_When_Strings_Match()
        {
            // Arrange
            string value1 = "password123";
            string value2 = "password123";
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            var result = ValidatorExtensions.Matches(value1, value2, errorContext, out var error);

            // Assert
            Assert.True(result);
            Assert.Null(error);
        }

        [Fact]
        public void Should_Return_True_And_No_Error_When_Both_Are_Empty()
        {
            // Arrange
            string value1 = string.Empty;
            string value2 = string.Empty;
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            var result = ValidatorExtensions.Matches(value1, value2, errorContext, out var error);

            // Assert
            Assert.True(result);
            Assert.Null(error);
        }


        private ErrorContext DefineErrorContext(string? fieldName = null, string? confirmFieldName = null)
        {
            return new ErrorContext(
                layer: "ResultPattern",
                serviceName: "ValidatorExtensions",
                methodName: "Test",
                operation: OperationType.Create,
                entityName: "User",
                fieldName: fieldName,
                confirmFieldName: confirmFieldName);
        }
    }
}
