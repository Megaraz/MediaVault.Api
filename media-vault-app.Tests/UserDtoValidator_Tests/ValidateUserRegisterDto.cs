using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Validation;
using media_vault_app.Application.Validators.User;
using Megaraz.ResultPattern;
using Xunit.Abstractions;

namespace media_vault_app.Tests.UserDtoValidator_Tests
{
    public class ValidateUserRegisterDto
    {

        private readonly ITestOutputHelper _output;

        public ValidateUserRegisterDto(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void IsValidRegisterDto_Should_ReturnFalse_And_Errors_For_NonMatching_Password_And_ConfirmPassword()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", "test@mail.com", "test@mail.com", "Test@4321", "Test@1234");

            var errorContext = DefineErrorContext(fieldName: "Password");

            // Act
            var result = userDtoValidator.IsValidCreateDto(userDto, errorContext, out var errors);

            // Assert
            Assert.False(result);
            Assert.NotNull(errors);

            var nonMatchingError = Assert.Single(errors);

            Assert.False(string.IsNullOrWhiteSpace(nonMatchingError.Code));
            Assert.NotEqual(ErrorType.None, nonMatchingError.Type);
            Assert.False(string.IsNullOrWhiteSpace(nonMatchingError.Description));
            Assert.Equal(ValidationErrorType.NonMatchingValues, nonMatchingError.ValidationErrorType);

            _output.WriteLine(nonMatchingError.Code);
            _output.WriteLine(nonMatchingError.Description);
        }

        [Fact]
        public void IsValidRegisterDto_Should_ReturnFalse_And_Errors_For_NonMatching_Email_And_ConfirmEmail()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", "test@mail.com", "t@mail.com", "Test@1234", "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidCreateDto(userDto, errorContext, out var errors);

            // Assert
            Assert.False(result);
            Assert.NotNull(errors);

            var nonMatchingError = Assert.Single(errors);

            Assert.False(string.IsNullOrWhiteSpace(nonMatchingError.Code));
            Assert.NotEqual(ErrorType.None, nonMatchingError.Type);
            Assert.False(string.IsNullOrWhiteSpace(nonMatchingError.Description));
            Assert.Equal(ValidationErrorType.NonMatchingValues, nonMatchingError.ValidationErrorType);

            _output.WriteLine(nonMatchingError.Code);
            _output.WriteLine(nonMatchingError.Description);
        }



        [Fact]
        public void IsValidRegisterDto_Should_ReturnTrue_And_NoErrors_When_AllFieldsAreValid()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();
            var errorContext = DefineErrorContext();
            var userDto = CreateValidUserRegisterDto();

            // Act
            var result = userDtoValidator.IsValidCreateDto(userDto, errorContext, out var errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void IsValidRegisterDto_Should_RejectOversizedUsername()
        {
            var userDtoValidator = new UserDtoValidator();
            var userDto = new UserRegisterDto(
                new string('u', MediaVaultWriteValidationPolicy.UserNameMaxLength + 1),
                "testuser@example.com",
                "testuser@example.com",
                "Test@1234",
                "Test@1234");

            var result = userDtoValidator.IsValidCreateDto(userDto, DefineErrorContext(), out var errors);

            Assert.False(result);
            Assert.Contains(errors, error => error.ValidationErrorType == ValidationErrorType.TooLong);
        }

        [Theory]
        [InlineData("not-an-email")]
        [InlineData("user@")]
        public void IsValidRegisterDto_Should_RejectInvalidEmailFormat(string email)
        {
            var userDtoValidator = new UserDtoValidator();
            var userDto = new UserRegisterDto(
                "testuser",
                email,
                email,
                "Test@1234",
                "Test@1234");

            var result = userDtoValidator.IsValidCreateDto(userDto, DefineErrorContext(), out var errors);

            Assert.False(result);
            Assert.Contains(
                errors,
                error => error.FieldName == nameof(UserRegisterDto.Email) &&
                          error.ValidationErrorType == ValidationErrorType.InvalidFormat);
        }

        [Fact]
        public void IsValidRegisterDto_Should_ReturnFalse_And_Errors_When_UserRegisterDtoIsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();
            UserRegisterDto? userDto = null;
            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidCreateDto(userDto!, errorContext, out var errors);

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
        public void IsValidRegisterDto_Should_ReturnFalse_And_Errors_When_ConfirmPassword_IsNullOrWhiteSpace(string? value)
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();
            UserRegisterDto userDto = new("Testuser", "test@mail.com", "test@mail.com", "Test@1234", value!);
            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidCreateDto(userDto, errorContext, out var errors);

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
        public void IsValidRegisterDto_Should_ReturnFalse_And_Errors_When_Password_IsNullOrWhiteSpace(string? value)
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();
            UserRegisterDto userDto = new("Testuser", "test@mail.com", "test@mail.com", value!, "Test@1234");
            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidCreateDto(userDto, errorContext, out var errors);

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
        public void IsValidRegisterDto_Should_ReturnFalse_And_Errors_When_ConfirmEmail_IsNullOrWhiteSpace(string? value)
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();
            UserRegisterDto userDto = new("Testuser", "test@mail.com", value!, "Test@1234", "Test@1234");
            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidCreateDto(userDto, errorContext, out var errors);

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
        public void IsValidRegisterDto_Should_ReturnFalse_And_Errors_When_Email_IsNullOrWhiteSpace(string? value)
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();
            UserRegisterDto userDto = new("Testuser", value!, "test@mail.com", "Test@1234", "Test@1234");
            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidCreateDto(userDto, errorContext, out var errors);

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
        public void IsValidRegisterDto_Should_ReturnFalse_And_Errors_When_UserName_IsNullOrWhiteSpace(string? value)
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();
            UserRegisterDto userDto = new(value!, "test@mail.com", "test@mail.com", "Test@1234", "Test@1234");
            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidCreateDto(userDto, errorContext, out var errors);

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


        private UserRegisterDto CreateValidUserRegisterDto()
        {
            return new UserRegisterDto(
                Username: "testuser",
                Email: "testuser@example.com",
                ConfirmEmail: "testuser@example.com",
                Password: "Test@1234",
                ConfirmPassword: "Test@1234"
                );
        }

        private ErrorContext DefineErrorContext(string? fieldName = null)
        {
            return new ErrorContext(
                operation: OperationType.Create,
                entityName: "User",
                fieldName: fieldName);
        }
    }
}
