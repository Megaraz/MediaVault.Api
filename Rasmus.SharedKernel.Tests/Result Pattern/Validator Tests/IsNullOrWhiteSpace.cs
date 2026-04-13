using System;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class IsNullOrWhiteSpace
    {
        // --- Overload with fieldName parameter ---

        [Fact]
        public void Should_Return_True_And_Error_For_Null_String_With_FieldName()
        {
            // Arrange
            string? value = null;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNullOrWhiteSpace(value!, "Username", errorContext, out var error);

            // Assert
            Assert.True(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Return_True_And_Error_For_Invalid_String_With_FieldName(string value)
        {
            // Arrange
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var error);

            // Assert
            Assert.True(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
        }

        [Fact]
        public void Should_Return_False_And_No_Error_For_Valid_String_With_FieldName()
        {
            // Arrange
            string value = "hello";
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.Null(error);
        }

        // --- Overload without fieldName parameter ---

        [Fact]
        public void Should_Return_True_And_Error_For_Null_String_Without_FieldName()
        {
            // Arrange
            string? value = null;
            var errorContext = DefineErrorContext("Username");

            // Act
            var result = ValidatorExtensions.IsNullOrWhiteSpace(value!, errorContext, out var error);

            // Assert
            Assert.True(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Return_True_And_Error_For_Invalid_String_Without_FieldName(string value)
        {
            // Arrange
            var errorContext = DefineErrorContext("Username");

            // Act
            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var error);

            // Assert
            Assert.True(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
        }

        [Fact]
        public void Should_Return_False_And_No_Error_For_Valid_String_Without_FieldName()
        {
            // Arrange
            string value = "hello";
            var errorContext = DefineErrorContext("Username");

            // Act
            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var error);

            // Assert
            Assert.False(result);
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
