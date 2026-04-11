using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class IsValidId
    {

        [Fact]
        public void Should_Return_False_For_Null_Integer()
        {
            // Arrange
            int? id = null;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out _);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Null_Integer()
        {
            // Arrange
            int? id = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }


        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Null_Integer()
        {
            // Arrange
            int? id = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Null_Integer()
        {
            // Arrange
            int? id = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }
        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Null_Integer()
        {
            // Arrange
            int? id = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_False_For_Negative_Integer()
        {
            // Arrange
            int id = -1;
            var errorContext = DefineErrorContext();
            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out _);
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Negative_Integer()
        {
            // Arrange
            int id = -1;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Negative_Integer()
        {
            // Arrange
            int id = -1;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Negative_Integer()
        {
            // Arrange
            int id = -1;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Negative_Integer()
        {
            // Arrange
            int id = -1;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_False_For_Default_Integer()
        {
            // Arrange
            int id = default;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out _);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Default_Integer()
        {
            // Arrange
            int id = default;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Default_Integer()
        {
            // Arrange
            int id = default;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Default_Integer()
        {
            // Arrange
            int id = default;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Default_Integer()
        {
            // Arrange
            int id = default;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_True_For_Valid_Integer()
        {
            // Arrange
            int id = 123;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_Return_False_For_Null_String()
        {
            // Arrange
            string id = null!;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out _);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Null_String()
        {
            // Arrange
            string id = null!;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Null_String()
        {
            // Arrange
            string id = null!;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Null_String()
        {
            // Arrange
            string id = null!;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Null_String()
        {
            // Arrange
            string id = null!;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_False_For_Empty_String()
        {
            // Arrange
            string id = string.Empty;
            var errorContext = DefineErrorContext();
            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out _);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Empty_String()
        {
            // Arrange
            string id = string.Empty;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Empty_String()
        {
            // Arrange
            string id = string.Empty;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Empty_String()
        {
            // Arrange
            string id = string.Empty;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Empty_String()
        {
            // Arrange
            string id = string.Empty;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_False_For_Whitespace_String()
        {
            // Arrange
            string id = "   ";
            var errorContext = DefineErrorContext();
            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out _);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Whitespace_String()
        {
            // Arrange
            string id = "   ";
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Whitespace_String()
        {
            // Arrange
            string id = "   ";
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Whitespace_String()
        {
            // Arrange
            string id = "   ";
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Whitespace_String()
        {
            // Arrange
            string id = "   ";
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_False_For_Empty_Guid()
        {
            // Arrange
            Guid id = Guid.Empty;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out _);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Empty_Guid()
        {
            // Arrange
            Guid id = Guid.Empty;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Empty_Guid()
        {
            // Arrange
            Guid id = Guid.Empty;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Empty_Guid()
        {
            // Arrange
            Guid id = Guid.Empty;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Empty_Guid()
        {
            // Arrange
            Guid id = Guid.Empty;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_False_For_Null_Guid()
        {
            // Arrange
            Guid? id = null;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out _);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Should_RefOut_ValidationError_For_Null_Guid()
        {
            // Arrange
            Guid? id = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Null_Guid()
        {
            // Arrange
            Guid? id = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Code));
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_ErrorTypeNotNone_For_Null_Guid()
        {
            // Arrange
            Guid? id = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.NotEqual(ErrorType.None, validationError.Type);
        }

        [Fact]
        public void Should_RefOut_ValidationError_With_DescriptionNotNullOrWhiteSpace_For_Null_Guid()
        {
            // Arrange
            Guid? id = null;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.IsValidId(id, errorContext, out var validationError);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(validationError.Description));
        }

        [Fact]
        public void Should_Return_True_For_Valid_Guid()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out _);

            // Assert
            Assert.True(result);
        }


        [Fact]
        public void Should_Return_True_For_Valid_String()
        {
            // Arrange
            string id = "valid-id";
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out _);

            // Assert
            Assert.True(result);
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
