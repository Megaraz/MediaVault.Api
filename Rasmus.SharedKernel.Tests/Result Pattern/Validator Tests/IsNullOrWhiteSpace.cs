using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class IsNullOrWhiteSpace
    {

        // --- Overload with fieldName parameter ---

        [Fact]
        public void Should_Return_True_For_Null_String_With_FieldName()
        {
            // Arrange
            string value = null!;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Null_String_With_FieldName()
        {
            // Arrange
            string value = null!;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Null_String_With_FieldName()
        {
            // Arrange
            string value = null!;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Null_String_With_FieldName()
        {
            // Arrange
            string value = null!;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Null_String_With_FieldName()
        {
            // Arrange
            string value = null!;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_True_For_Empty_String_With_FieldName()
        {
            // Arrange
            string value = string.Empty;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Empty_String_With_FieldName()
        {
            // Arrange
            string value = string.Empty;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Empty_String_With_FieldName()
        {
            // Arrange
            string value = string.Empty;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Empty_String_With_FieldName()
        {
            // Arrange
            string value = string.Empty;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Empty_String_With_FieldName()
        {
            // Arrange
            string value = string.Empty;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_True_For_Whitespace_String_With_FieldName()
        {
            // Arrange
            string value = "   ";
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Whitespace_String_With_FieldName()
        {
            // Arrange
            string value = "   ";
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Whitespace_String_With_FieldName()
        {
            // Arrange
            string value = "   ";
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Whitespace_String_With_FieldName()
        {
            // Arrange
            string value = "   ";
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Whitespace_String_With_FieldName()
        {
            // Arrange
            string value = "   ";
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_False_For_Valid_String_With_FieldName()
        {
            // Arrange
            string value = "hello";
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, "Username", errorContext, out _);

            // Assert
            Assert.False(result);
        }

        // --- Overload without fieldName parameter ---

        [Fact]
        public void Should_Return_True_For_Null_String_Without_FieldName()
        {
            // Arrange
            string value = null!;
            var errorContext = DefineErrorContext("Username");

            // Act
            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Null_String_Without_FieldName()
        {
            // Arrange
            string value = null!;
            var errorContext = DefineErrorContext("Username");

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Null_String_Without_FieldName()
        {
            // Arrange
            string value = null!;
            var errorContext = DefineErrorContext("Username");

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Null_String_Without_FieldName()
        {
            // Arrange
            string value = null!;
            var errorContext = DefineErrorContext("Username");

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Null_String_Without_FieldName()
        {
            // Arrange
            string value = null!;
            var errorContext = DefineErrorContext("Username");

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_True_For_Empty_String_Without_FieldName()
        {
            // Arrange
            string value = string.Empty;
            var errorContext = DefineErrorContext("Username");

            // Act
            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Empty_String_Without_FieldName()
        {
            // Arrange
            string value = string.Empty;
            var errorContext = DefineErrorContext("Username");

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Empty_String_Without_FieldName()
        {
            // Arrange
            string value = string.Empty;
            var errorContext = DefineErrorContext("Username");

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Empty_String_Without_FieldName()
        {
            // Arrange
            string value = string.Empty;
            var errorContext = DefineErrorContext("Username");

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Empty_String_Without_FieldName()
        {
            // Arrange
            string value = string.Empty;
            var errorContext = DefineErrorContext("Username");

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_True_For_Whitespace_String_Without_FieldName()
        {
            // Arrange
            string value = "   ";
            var errorContext = DefineErrorContext("Username");

            // Act
            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Whitespace_String_Without_FieldName()
        {
            // Arrange
            string value = "   ";
            var errorContext = DefineErrorContext("Username");

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Whitespace_String_Without_FieldName()
        {
            // Arrange
            string value = "   ";
            var errorContext = DefineErrorContext("Username");

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Whitespace_String_Without_FieldName()
        {
            // Arrange
            string value = "   ";
            var errorContext = DefineErrorContext("Username");

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Whitespace_String_Without_FieldName()
        {
            // Arrange
            string value = "   ";
            var errorContext = DefineErrorContext("Username");

            // Act
            ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_False_For_Valid_String_Without_FieldName()
        {
            // Arrange
            string value = "hello";
            var errorContext = DefineErrorContext("Username");

            // Act
            var result = ValidatorExtensions.IsNullOrWhiteSpace(value, errorContext, out _);

            // Assert
            Assert.False(result);
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
