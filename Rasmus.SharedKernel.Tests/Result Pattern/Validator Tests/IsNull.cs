using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class IsNull
    {

        [Fact]
        public void Should_Return_True_For_Null_Object()
        {
            // Arrange
            object? value = null;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNull(value, errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Null_Object()
        {
            // Arrange
            object? value = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNull(value, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Null_Object()
        {
            // Arrange
            object? value = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNull(value, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Null_Object()
        {
            // Arrange
            object? value = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNull(value, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Null_Object()
        {
            // Arrange
            object? value = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNull(value, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_True_For_Null_String()
        {
            // Arrange
            string? value = null;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNull(value, errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Null_String()
        {
            // Arrange
            string? value = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNull(value, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Null_String()
        {
            // Arrange
            string? value = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNull(value, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Null_String()
        {
            // Arrange
            string? value = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNull(value, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Null_String()
        {
            // Arrange
            string? value = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNull(value, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_False_For_NonNull_Object()
        {
            // Arrange
            object value = new object();
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNull(value, errorContext, out _);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Should_Return_False_For_NonNull_String()
        {
            // Arrange
            string value = "hello";
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNull(value, errorContext, out _);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Should_Return_False_For_NonNull_Integer()
        {
            // Arrange
            int? value = 42;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNull(value, errorContext, out _);

            // Assert
            Assert.False(result);
        }



        private ErrorContext DefineErrorContext()
        {
            return new ErrorContext(
                layer: "ResultPattern",
                serviceName: "ValidatorExtensions",
                methodName: "Test",
                operation: OperationType.Create,
                entityName: "User",
                fieldName: null,
                confirmFieldName: null);
        }
    }
}
