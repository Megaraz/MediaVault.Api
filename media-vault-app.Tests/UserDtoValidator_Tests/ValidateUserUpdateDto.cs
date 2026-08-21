using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Validation;
using media_vault_app.Application.Validators.User;
using Megaraz.ResultPattern;

namespace media_vault_app.Tests.UserDtoValidator_Tests
{
    public class ValidateUserUpdateDto
    {
        [Fact]
        public void IsValidUpdateDto_Should_ReturnTrue_And_NoErrors_When_AllFieldsAreValid()
        {
            var validator = new UserDtoValidator();
            var dto = new UserUpdateDto
            {
                ExpectedVersion = 1,
                UserName = "updated-user",
                Email = "updated@example.com"
            };

            var result = validator.IsValidUpdateDto(dto, DefineErrorContext(), out var errors);

            Assert.True(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void IsValidUpdateDto_Should_RejectOversizedUsername()
        {
            var validator = new UserDtoValidator();
            var dto = new UserUpdateDto
            {
                ExpectedVersion = 1,
                UserName = new string('u', MediaVaultWriteValidationPolicy.UserNameMaxLength + 1),
                Email = "updated@example.com"
            };

            var result = validator.IsValidUpdateDto(dto, DefineErrorContext(), out var errors);

            Assert.False(result);
            Assert.Contains(errors, error => error.ValidationErrorType == ValidationErrorType.TooLong);
        }

        [Fact]
        public void IsValidUpdateDto_Should_RejectInvalidEmailFormat()
        {
            var validator = new UserDtoValidator();
            var dto = new UserUpdateDto
            {
                ExpectedVersion = 1,
                UserName = "updated-user",
                Email = "not-an-email"
            };

            var result = validator.IsValidUpdateDto(dto, DefineErrorContext(), out var errors);

            Assert.False(result);
            Assert.Contains(
                errors,
                error => error.FieldName == nameof(UserUpdateDto.Email) &&
                          error.ValidationErrorType == ValidationErrorType.InvalidFormat);
        }

        [Fact]
        public void IsValidUpdateDto_Should_ReturnFalse_And_RequiredError_When_DtoIsNull()
        {
            var validator = new UserDtoValidator();

            var result = validator.IsValidUpdateDto(null!, DefineErrorContext(), out var errors);

            Assert.False(result);
            var error = Assert.Single(errors);
            Assert.Equal(ErrorType.Validation, error.Type);
            Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
        }

        [Fact]
        public void IsValidUpdateDto_Should_RejectMissingExpectedVersion()
        {
            var validator = new UserDtoValidator();
            var dto = new UserUpdateDto
            {
                UserName = "updated-user",
                Email = "updated@example.com"
            };

            var result = validator.IsValidUpdateDto(dto, DefineErrorContext(), out var errors);

            Assert.False(result);
            var error = Assert.Single(errors);
            Assert.Equal(nameof(UserUpdateDto.ExpectedVersion), error.FieldName);
            Assert.Equal(ValidationErrorType.OutOfRange, error.ValidationErrorType);
        }

        [Theory]
        [InlineData("", "updated@example.com")]
        [InlineData("   ", "updated@example.com")]
        [InlineData(null, "updated@example.com")]
        [InlineData("updated-user", "")]
        [InlineData("updated-user", "   ")]
        [InlineData("updated-user", null)]
        public void IsValidUpdateDto_Should_ReturnFalse_And_RequiredError_When_ARequiredFieldIsMissing(string? userName, string? email)
        {
            var validator = new UserDtoValidator();
            var dto = new UserUpdateDto
            {
                ExpectedVersion = 1,
                UserName = userName!,
                Email = email!
            };

            var result = validator.IsValidUpdateDto(dto, DefineErrorContext(), out var errors);

            Assert.False(result);
            var error = Assert.Single(errors);
            Assert.Equal(ErrorType.Validation, error.Type);
            Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
        }

        [Fact]
        public void IsValidUpdateDto_Should_ReturnAllRequiredErrors_When_MultipleFieldsAreMissing()
        {
            var validator = new UserDtoValidator();
            var dto = new UserUpdateDto
            {
                ExpectedVersion = 1,
                UserName = "",
                Email = " "
            };

            var result = validator.IsValidUpdateDto(dto, DefineErrorContext(), out var errors);

            Assert.False(result);
            Assert.Equal(2, errors.Count);
            Assert.All(errors, error =>
            {
                Assert.Equal(ErrorType.Validation, error.Type);
                Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
            });
        }

        private static ErrorContext DefineErrorContext(string? fieldName = null)
        {
            return new ErrorContext(
                operation: OperationType.Update,
                entityName: "User",
                fieldName: fieldName);
        }
    }
}
