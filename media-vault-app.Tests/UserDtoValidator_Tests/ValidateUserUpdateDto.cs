using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Validators.User;
using Rasmus.SharedKernel.ResultPattern;

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
                UserName = "updated-user",
                Email = "updated@example.com"
            };

            var result = validator.IsValidUpdateDto(dto, DefineErrorContext(), out var errors);

            Assert.True(result);
            Assert.Empty(errors);
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
                Layer: "Service",
                ServiceName: "UserWriteService",
                MethodName: "UpdateAsync",
                Operation: OperationType.Update,
                EntityName: "User",
                FieldName: fieldName);
        }
    }
}