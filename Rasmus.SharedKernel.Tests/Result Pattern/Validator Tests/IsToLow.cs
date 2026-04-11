using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class IsToLow
    {

        [Fact]
        public void Should_Return_False_When_Value_Is_Below_MinValue()
        {
            // Arrange
            int value = 3;
            int minValue = 5;
            var errorContext = DefineErrorContext("Age");

            // Act
            var result = ValidatorExtensions.IsToLow(value, minValue, errorContext, out _);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_When_Value_Is_Below_MinValue()
        {
            // Arrange
            int value = 3;
            int minValue = 5;
            var errorContext = DefineErrorContext("Age");

            // Act
            ValidatorExtensions.IsToLow(value, minValue, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_When_Value_Is_Below_MinValue()
        {
            // Arrange
            int value = 3;
            int minValue = 5;
            var errorContext = DefineErrorContext("Age");

            // Act
            ValidatorExtensions.IsToLow(value, minValue, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_When_Value_Is_Below_MinValue()
        {
            // Arrange
            int value = 3;
            int minValue = 5;
            var errorContext = DefineErrorContext("Age");

            // Act
            ValidatorExtensions.IsToLow(value, minValue, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_When_Value_Is_Below_MinValue()
        {
            // Arrange
            int value = 3;
            int minValue = 5;
            var errorContext = DefineErrorContext("Age");

            // Act
            ValidatorExtensions.IsToLow(value, minValue, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_False_When_Value_Is_Negative()
        {
            // Arrange
            int value = -1;
            int minValue = 0;
            var errorContext = DefineErrorContext("Age");

            // Act
            var result = ValidatorExtensions.IsToLow(value, minValue, errorContext, out _);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_When_Value_Is_Negative()
        {
            // Arrange
            int value = -1;
            int minValue = 0;
            var errorContext = DefineErrorContext("Age");

            // Act
            ValidatorExtensions.IsToLow(value, minValue, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_When_Value_Is_Negative()
        {
            // Arrange
            int value = -1;
            int minValue = 0;
            var errorContext = DefineErrorContext("Age");

            // Act
            ValidatorExtensions.IsToLow(value, minValue, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_When_Value_Is_Negative()
        {
            // Arrange
            int value = -1;
            int minValue = 0;
            var errorContext = DefineErrorContext("Age");

            // Act
            ValidatorExtensions.IsToLow(value, minValue, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_When_Value_Is_Negative()
        {
            // Arrange
            int value = -1;
            int minValue = 0;
            var errorContext = DefineErrorContext("Age");

            // Act
            ValidatorExtensions.IsToLow(value, minValue, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_True_When_Value_Equals_MinValue()
        {
            // Arrange
            int value = 5;
            int minValue = 5;
            var errorContext = DefineErrorContext("Age");

            // Act
            var result = ValidatorExtensions.IsToLow(value, minValue, errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_Return_True_When_Value_Is_Above_MinValue()
        {
            // Arrange
            int value = 10;
            int minValue = 5;
            var errorContext = DefineErrorContext("Age");

            // Act
            var result = ValidatorExtensions.IsToLow(value, minValue, errorContext, out _);

            // Assert
            Assert.True(result);
        }



        private ErrorContext DefineErrorContext(string? fieldName = null)
        {
            return new ErrorContext(
                layer: "ResultPattern",
                serviceName: "ValidatorExtensions",
                methodName: "Test",
                operation: OperationType.Create,
                entityName: "User",
                fieldName: fieldName,
                confirmFieldName: null);
        }
    }
}
