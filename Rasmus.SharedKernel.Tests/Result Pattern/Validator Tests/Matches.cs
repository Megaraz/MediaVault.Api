using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class Matches
    {

        [Fact]
        public void Should_Return_False_When_Strings_Do_Not_Match()
        {
            // Arrange
            string value1 = "password123";
            string value2 = "password456";
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            var result = ValidatorExtensions.Matches(value1, value2, errorContext, out _);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_When_Strings_Do_Not_Match()
        {
            // Arrange
            string value1 = "password123";
            string value2 = "password456";
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            ValidatorExtensions.Matches(value1, value2, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_When_Strings_Do_Not_Match()
        {
            // Arrange
            string value1 = "password123";
            string value2 = "password456";
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            ValidatorExtensions.Matches(value1, value2, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_When_Strings_Do_Not_Match()
        {
            // Arrange
            string value1 = "password123";
            string value2 = "password456";
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            ValidatorExtensions.Matches(value1, value2, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_When_Strings_Do_Not_Match()
        {
            // Arrange
            string value1 = "password123";
            string value2 = "password456";
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            ValidatorExtensions.Matches(value1, value2, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_False_When_CaseDiffers()
        {
            // Arrange
            string value1 = "Password";
            string value2 = "password";
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            var result = ValidatorExtensions.Matches(value1, value2, errorContext, out _);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_When_CaseDiffers()
        {
            // Arrange
            string value1 = "Password";
            string value2 = "password";
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            ValidatorExtensions.Matches(value1, value2, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_When_CaseDiffers()
        {
            // Arrange
            string value1 = "Password";
            string value2 = "password";
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            ValidatorExtensions.Matches(value1, value2, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_When_CaseDiffers()
        {
            // Arrange
            string value1 = "Password";
            string value2 = "password";
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            ValidatorExtensions.Matches(value1, value2, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_When_CaseDiffers()
        {
            // Arrange
            string value1 = "Password";
            string value2 = "password";
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            ValidatorExtensions.Matches(value1, value2, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_True_When_Strings_Match()
        {
            // Arrange
            string value1 = "password123";
            string value2 = "password123";
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            var result = ValidatorExtensions.Matches(value1, value2, errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_Return_True_When_Both_Are_Empty()
        {
            // Arrange
            string value1 = string.Empty;
            string value2 = string.Empty;
            var errorContext = DefineErrorContext("Password", "ConfirmPassword");

            // Act
            var result = ValidatorExtensions.Matches(value1, value2, errorContext, out _);

            // Assert
            Assert.True(result);
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
