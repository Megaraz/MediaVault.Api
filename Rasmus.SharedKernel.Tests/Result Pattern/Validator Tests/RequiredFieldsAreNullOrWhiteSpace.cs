using System;
using System.Collections.Generic;
using System.Linq;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class RequiredFieldsAreNullOrWhiteSpace
    {
        [Fact]
        public void Should_Return_True_And_Errors_For_Null_Collection()
        {
            // Arrange
            IEnumerable<(string FieldName, string Value)> requiredValues = null!;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var errors);

            // Assert
            Assert.True(result);
            Assert.NotNull(errors);
            Assert.NotEmpty(errors);
            Assert.All(errors, e =>
            {
                Assert.False(string.IsNullOrWhiteSpace(e.Code));
                Assert.NotEqual(ErrorType.None, e.Type);
                Assert.False(string.IsNullOrWhiteSpace(e.Description));
                Assert.Equal(ValidationErrorType.Required, e.ValidationErrorType);
            });
        }

        [Fact]
        public void Should_Return_True_And_Errors_When_All_Fields_Are_Null()
        {
            // Arrange
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", null!),
                ("Email", null!)
            };
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var errors);

            // Assert
            Assert.True(result);
            Assert.Equal(2, errors.Count());
            Assert.All(errors, e =>
            {
                Assert.False(string.IsNullOrWhiteSpace(e.Code));
                Assert.NotEqual(ErrorType.None, e.Type);
                Assert.False(string.IsNullOrWhiteSpace(e.Description));
                Assert.Equal(ValidationErrorType.Required, e.ValidationErrorType);
            });
        }

        [Fact]
        public void Should_Return_True_When_All_Fields_Are_Empty()
        {
            // Arrange
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", string.Empty),
                ("Email", string.Empty)
            };
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_Return_True_When_All_Fields_Are_Whitespace()
        {
            // Arrange
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", "   "),
                ("Email", "   ")
            };
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_Return_True_And_Single_Error_When_One_Field_Is_Invalid()
        {
            // Arrange
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", "validuser"),
                ("Email", string.Empty)
            };
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var errors);

            // Assert
            Assert.True(result);
            Assert.Single(errors);
        }

        [Fact]
        public void Should_Return_False_And_No_Errors_When_All_Fields_Are_Valid()
        {
            // Arrange
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", "validuser"),
                ("Email", "test@example.com")
            };
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var errors);

            // Assert
            Assert.False(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_Return_False_And_No_Errors_For_Empty_Collection()
        {
            // Arrange
            var requiredValues = new List<(string FieldName, string Value)>();
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var errors);

            // Assert
            Assert.False(result);
            Assert.Empty(errors);
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
