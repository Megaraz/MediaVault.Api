using System;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class IsValidId
    {
        // --- Invalid integer scenarios ---

        [Fact]
        public void Should_Return_False_And_Error_For_Null_Integer()
        {
            // Arrange
            int? id = null;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Should_Return_False_And_Error_For_Invalid_Integer(int id)
        {
            // Arrange
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
        }

        [Fact]
        public void Should_Return_True_And_No_Error_For_Valid_Integer()
        {
            // Arrange
            int id = 123;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out var error);

            // Assert
            Assert.True(result);
            Assert.Null(error);
        }

        // --- Invalid string scenarios ---

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Return_False_And_Error_For_Invalid_String(string id)
        {
            // Arrange
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
        }

        [Fact]
        public void Should_Return_False_And_Error_For_Null_String()
        {
            // Arrange
            string? id = null;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
        }

        [Fact]
        public void Should_Return_True_And_No_Error_For_Valid_String()
        {
            // Arrange
            string id = "valid-id";
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out var error);

            // Assert
            Assert.True(result);
            Assert.Null(error);
        }

        // --- Invalid Guid scenarios ---

        [Fact]
        public void Should_Return_False_And_Error_For_Empty_Guid()
        {
            // Arrange
            Guid id = Guid.Empty;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
        }

        [Fact]
        public void Should_Return_False_And_Error_For_Null_Guid()
        {
            // Arrange
            Guid? id = null;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
        }

        [Fact]
        public void Should_Return_True_And_No_Error_For_Valid_Guid()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsValidId(id, errorContext, out var error);

            // Assert
            Assert.True(result);
            Assert.Null(error);
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
