using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests.Result_Pattern.Validator_Tests
{
    public class RequiredFieldsAreNullOrWhiteSpace
    {

        [Fact]
        public void Should_Return_True_For_Null_Collection()
        {
            // Arrange
            IEnumerable<(string FieldName, string Value)> requiredValues = null!;
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_For_Null_Collection()
        {
            // Arrange
            IEnumerable<(string FieldName, string Value)> requiredValues = null!;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var validationErrors);

            // Assert
            Assert.NotNull(validationErrors);
            Assert.NotEmpty(validationErrors);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_Null_Collection()
        {
            // Arrange
            IEnumerable<(string FieldName, string Value)> requiredValues = null!;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var validationErrors);

            // Assert
            Assert.All(validationErrors, e => Assert.False(string.IsNullOrWhiteSpace(e.Code)));
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_ErrorTypeNotNone_For_Null_Collection()
        {
            // Arrange
            IEnumerable<(string FieldName, string Value)> requiredValues = null!;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var validationErrors);

            // Assert
            Assert.All(validationErrors, e => Assert.NotEqual(ErrorType.None, e.Type));
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_DescriptionNotNullOrWhiteSpace_For_Null_Collection()
        {
            // Arrange
            IEnumerable<(string FieldName, string Value)> requiredValues = null!;
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var validationErrors);

            // Assert
            Assert.All(validationErrors, e => Assert.False(string.IsNullOrWhiteSpace(e.Description)));
        }

        [Fact]
        public void Should_Return_True_When_All_Fields_Are_Null()
        {
            // Arrange
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", null!),
                ("Email", null!)
            };
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_For_All_Null_Fields()
        {
            // Arrange
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", null!),
                ("Email", null!)
            };
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var validationErrors);

            // Assert
            Assert.NotNull(validationErrors);
            Assert.Equal(2, validationErrors.Count());
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_NonEmptyErrorCode_For_All_Null_Fields()
        {
            // Arrange
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", null!),
                ("Email", null!)
            };
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var validationErrors);

            // Assert
            Assert.All(validationErrors, e => Assert.False(string.IsNullOrWhiteSpace(e.Code)));
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_ErrorTypeNotNone_For_All_Null_Fields()
        {
            // Arrange
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", null!),
                ("Email", null!)
            };
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var validationErrors);

            // Assert
            Assert.All(validationErrors, e => Assert.NotEqual(ErrorType.None, e.Type));
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_With_DescriptionNotNullOrWhiteSpace_For_All_Null_Fields()
        {
            // Arrange
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", null!),
                ("Email", null!)
            };
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var validationErrors);

            // Assert
            Assert.All(validationErrors, e => Assert.False(string.IsNullOrWhiteSpace(e.Description)));
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
        public void Should_Return_True_When_Some_Fields_Are_Invalid()
        {
            // Arrange
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", "validuser"),
                ("Email", string.Empty)
            };
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_RefOut_SingleError_When_One_Field_Is_Invalid()
        {
            // Arrange
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", "validuser"),
                ("Email", string.Empty)
            };
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var validationErrors);

            // Assert
            Assert.Single(validationErrors);
        }

        [Fact]
        public void Should_Return_False_When_All_Fields_Are_Valid()
        {
            // Arrange
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", "validuser"),
                ("Email", "test@example.com")
            };
            var errorContext = DefineErrorContext();

            // Act
            var result = ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out _);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Should_RefOut_EmptyErrors_When_All_Fields_Are_Valid()
        {
            // Arrange
            var requiredValues = new List<(string FieldName, string Value)>
            {
                ("Username", "validuser"),
                ("Email", "test@example.com")
            };
            var errorContext = DefineErrorContext();

            // Act
            ValidatorExtensions.RequiredFieldsAreNullOrWhiteSpace(requiredValues, errorContext, out var validationErrors);

            // Assert
            Assert.Empty(validationErrors);
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
