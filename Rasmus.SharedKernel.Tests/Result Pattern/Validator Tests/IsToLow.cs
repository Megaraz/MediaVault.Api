using System;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class IsToLow
    {
        [Fact]
        public void Should_Return_False_And_Error_When_Value_Is_Below_MinValue()
        {
            // Arrange
            int value = 3;
            int minValue = 5;
            var errorContext = DefineErrorContext("Age");

            // Act
            var result = ValidatorExtensions.IsToLow(value, minValue, errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.TooShort, error.ValidationErrorType);
        }

        [Fact]
        public void Should_Return_False_And_Error_When_Value_Is_Negative()
        {
            // Arrange
            int value = -1;
            int minValue = 0;
            var errorContext = DefineErrorContext("Age");

            // Act
            var result = ValidatorExtensions.IsToLow(value, minValue, errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.TooShort, error.ValidationErrorType);
        }

        [Fact]
        public void Should_Return_False_And_Error_When_Value_Is_One_Below_MinValue()
        {
            // Arrange
            int value = 4;
            int minValue = 5;
            var errorContext = DefineErrorContext("Age");

            // Act
            var result = ValidatorExtensions.IsToLow(value, minValue, errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.NotNull(error);
            Assert.Equal(ValidationErrorType.TooShort, error.ValidationErrorType);
        }

        [Fact]
        public void Should_Return_True_And_No_Error_When_Value_Equals_MinValue()
        {
            // Arrange
            int value = 5;
            int minValue = 5;
            var errorContext = DefineErrorContext("Age");

            // Act
            var result = ValidatorExtensions.IsToLow(value, minValue, errorContext, out var error);

            // Assert
            Assert.True(result);
            Assert.Null(error);
        }

        [Fact]
        public void Should_Return_True_And_No_Error_When_Value_Is_Above_MinValue()
        {
            // Arrange
            int value = 10;
            int minValue = 5;
            var errorContext = DefineErrorContext("Age");

            // Act
            var result = ValidatorExtensions.IsToLow(value, minValue, errorContext, out var error);

            // Assert
            Assert.True(result);
            Assert.Null(error);
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
