using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Validators.User;
using Rasmus.SharedKernel.ResultPattern;
using Xunit.Abstractions;

namespace media_vault_app.Tests.UserDtoValidator_Tests
{
    public class ValidateUserLoginDto
    {

        private readonly ITestOutputHelper _output;

        public ValidateUserLoginDto(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void IsValidLoginDto_Should_ReturnTrue_And_NoErrors_When_AllFieldsAreValid()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();
            var errorContext = DefineErrorContext();
            var loginDto = new UserLoginDto("test@mail.com", "Test@1234");

            // Act
            var result = userDtoValidator.IsValidLoginDto(loginDto, errorContext, out var errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void IsValidLoginDto_Should_ReturnFalse_And_Errors_When_UserLoginDtoIsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();
            UserLoginDto? loginDto = null;
            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidLoginDto(loginDto!, errorContext, out var errors);

            // Assert
            Assert.False(result);
            Assert.NotEmpty(errors);
            Assert.All(errors, error =>
            {
                Assert.False(string.IsNullOrWhiteSpace(error.Code));
                Assert.NotEqual(ErrorType.None, error.Type);
                Assert.False(string.IsNullOrWhiteSpace(error.Description));
            });
            var requiredError = Assert.Single(errors, error => error.Type == ErrorType.Validation && error.ValidationErrorType == ValidationErrorType.Required);

            Assert.NotNull(requiredError);

            _output.WriteLine(requiredError.Code);
            _output.WriteLine(requiredError.Description);

        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void IsValidLoginDto_Should_ReturnFalse_And_Errors_When_UserNameOrEmail_IsNullOrWhiteSpace(string? value)
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();
            UserLoginDto loginDto = new(value!, "Test@1234");
            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidLoginDto(loginDto, errorContext, out var errors);

            // Assert
            Assert.False(result);
            Assert.NotNull(errors);
            Assert.NotEmpty(errors);

            Assert.All(errors, error =>
            {
                Assert.False(string.IsNullOrWhiteSpace(error.Code));
                Assert.NotEqual(ErrorType.None, error.Type);
                Assert.False(string.IsNullOrWhiteSpace(error.Description));
            });

            var requiredError = Assert.Single(errors, error => error.Type == ErrorType.Validation && error.ValidationErrorType == ValidationErrorType.Required);

            Assert.NotNull(requiredError);

            _output.WriteLine(requiredError.Code);
            _output.WriteLine(requiredError.Description);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void IsValidLoginDto_Should_ReturnFalse_And_Errors_When_Password_IsNullOrWhiteSpace(string? value)
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();
            UserLoginDto loginDto = new("test@mail.com", value!);
            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidLoginDto(loginDto, errorContext, out var errors);

            // Assert
            Assert.False(result);
            Assert.NotNull(errors);
            Assert.NotEmpty(errors);

            Assert.All(errors, error =>
            {
                Assert.False(string.IsNullOrWhiteSpace(error.Code));
                Assert.NotEqual(ErrorType.None, error.Type);
                Assert.False(string.IsNullOrWhiteSpace(error.Description));
            });

            var requiredError = Assert.Single(errors, error => error.Type == ErrorType.Validation && error.ValidationErrorType == ValidationErrorType.Required);

            Assert.NotNull(requiredError);

            _output.WriteLine(requiredError.Code);
            _output.WriteLine(requiredError.Description);

        }


        private ErrorContext DefineErrorContext(string? fieldName = null)
        {
            return new ErrorContext(
                Layer: "Service",
                ServiceName: "AuthService",
                MethodName: "LoginUserAsync",
                Operation: OperationType.Login,
                EntityName: "User",
                FieldName: fieldName);
        }
    }
}
