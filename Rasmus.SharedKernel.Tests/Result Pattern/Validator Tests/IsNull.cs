using System;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class IsNull
    {
        [Fact]
        public void Should_Return_True_And_Error_For_Null_Object()
        {
            // Arrange
            object? value = null;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNull(value, errorContext, out var error);

            // Assert
            Assert.True(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
        }

        [Fact]
        public void Should_Return_True_And_Error_For_Null_String()
        {
            // Arrange
            string? value = null;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNull(value, errorContext, out var error);

            // Assert
            Assert.True(result);
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.NotEqual(ErrorType.None, error.Type);
            Assert.False(string.IsNullOrWhiteSpace(error.Description));
            Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
        }

        [Fact]
        public void Should_Return_False_And_No_Error_For_NonNull_Object()
        {
            // Arrange
            object value = new object();
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNull(value, errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.Null(error);
        }

        [Fact]
        public void Should_Return_False_And_No_Error_For_NonNull_String()
        {
            // Arrange
            string value = "hello";
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNull(value, errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.Null(error);
        }

        [Fact]
        public void Should_Return_False_And_No_Error_For_NonNull_Integer()
        {
            // Arrange
            int? value = 42;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.IsNull(value, errorContext, out var error);

            // Assert
            Assert.False(result);
            Assert.Null(error);
        }


        private ErrorContext DefineErrorContext()
        {
            return new ErrorContext(
                Layer: "ResultPattern",
                ServiceName: "ValidatorExtensions",
                MethodName: "Test",
                Operation: OperationType.Create,
                EntityName: "User",
                FieldName: null,
                ConfirmFieldName: null);
        }
    }
}
