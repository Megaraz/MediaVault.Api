using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Validators.User;
using Rasmus.SharedKernel.ResultPattern;
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

        #region UserRegisterDto Validation Tests
        [Fact]
        // Happy Path
        public void Should_ReturnTrue_When_UserRegisterDtoIsNotNull_And_AllRequiredFieldsAreProvided()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();
            var errorContext = DefineErrorContext();
            var userDto = CreateValidUserRegisterDto();


            // Act
            var result = userDtoValidator.IsValidRegisterDto(userDto, errorContext, out _);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_ReturnFalse_When_UserRegisterDtoIsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto? userDto = null;

            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidRegisterDto(userDto!, errorContext, out _);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public void Should_RefOut_EmptyValidationErrorCollection_When_UserRegisterDtoIsNotNull_And_AllRequiredFieldsAreProvided()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();
            var errorContext = DefineErrorContext();
            var userDto = CreateValidUserRegisterDto();


            // Act
            userDtoValidator.IsValidRegisterDto(userDto, errorContext, out var validationErrors);

            // Assert
            Assert.Empty(validationErrors);
        }

        [Fact]
        public void Should_RefOut_ValidationErrors_When_UserRegisterDto_IsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto? userDto = null;

            var errorContext = DefineErrorContext();

            // Act
            userDtoValidator.IsValidRegisterDto(userDto!, errorContext, out var validationErrors);

            // Assert
            Assert.NotEmpty(validationErrors);

        }


        [Fact]
        public void ValidationErrors_ErrorCode_Should_NotBeNullOrWhiteSpace_When_UserRegisterDto_IsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto? userDto = null;

            var errorContext = DefineErrorContext();

            // Act
            userDtoValidator.IsValidRegisterDto(userDto!, errorContext, out var validationErrors);

            // Assert
            Assert.All(validationErrors, error => Assert.False(string.IsNullOrWhiteSpace(error.Code)));

        }

        [Fact]
        public void ValidationErrors_ErrorType_Should_NotBeNone_When_UserRegisterDto_IsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto? userDto = null;

            var errorContext = DefineErrorContext();

            // Act
            userDtoValidator.IsValidRegisterDto(userDto!, errorContext, out var validationErrors);

            // Assert
            Assert.All(validationErrors, error => Assert.NotEqual(ErrorType.None, error.Type));

        }


        [Fact]
        public void ValidationErrors_Description_Should_NotBeNullOrWhiteSpace_When_UserRegisterDto_IsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto? userDto = null;

            var errorContext = DefineErrorContext();

            // Act
            userDtoValidator.IsValidRegisterDto(userDto!, errorContext, out var validationErrors);

            // Assert
            Assert.All(validationErrors, error => Assert.False(string.IsNullOrWhiteSpace(error.Description)));

        }

        #endregion


        #region ConfirmPassword Validation Tests
        [Fact]
        public void Should_RefOut_ValidationError_When_RequiredField_ConfirmPassword_IsEmpty()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", "test@mail.com", "test@mail.com", "Test@1234", string.Empty);

            var errorContext = DefineErrorContext();

            // Act
            userDtoValidator.IsValidRegisterDto(userDto, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);

        }

        [Fact]
        public void Should_ReturnFalse_When_RequiredField_ConfirmPassword_IsEmpty()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", "test@mail.com", "test@mail.com", "Test@1234", string.Empty);

            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidRegisterDto(userDto, errorContext, out _);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public void Should_ReturnFalse_When_RequiredField_ConfirmPassword_IsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", "test@mail.com", "test@mail.com", "Test@1234", null!);

            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidRegisterDto(userDto, errorContext, out _);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public void Should_RefOut_ValidationError_When_RequiredField_ConfirmPassword_IsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", "test@mail.com", "test@mail.com", "Test@1234", null!);

            var errorContext = DefineErrorContext();

            // Act
            userDtoValidator.IsValidRegisterDto(userDto, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);

        }
        #endregion

        #region Password Validation Tests
        [Fact]
        public void Should_ReturnFalse_When_RequiredField_Password_IsEmpty()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", "test@mail.com", "test@mail.com", string.Empty, "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidRegisterDto(userDto, errorContext, out _);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public void Should_RefOut_ValidationError_When_RequiredField_Password_IsEmpty()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", "test@mail.com", "test@mail.com", string.Empty, "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            userDtoValidator.IsValidRegisterDto(userDto, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);

        }
        [Fact]
        public void Should_ReturnFalse_When_RequiredField_Password_IsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", "test@mail.com", "test@mail.com", null!, "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidRegisterDto(userDto, errorContext, out _);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public void Should_RefOut_ValidationError_When_RequiredField_Password_IsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", "test@mail.com", "test@mail.com", null!, "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            userDtoValidator.IsValidRegisterDto(userDto, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);

        }

        #endregion

        #region ConfirmEmail Validation Tests
        [Fact]
        public void Should_ReturnFalse_When_RequiredField_ConfirmEmail_IsEmpty()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", "test@mail.com", string.Empty, "Test@1234", "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidRegisterDto(userDto, errorContext, out _);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public void Should_RefOut_ValidationError_When_RequiredField_ConfirmEmail_IsEmpty()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", "test@mail.com", string.Empty, "Test@1234", "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            userDtoValidator.IsValidRegisterDto(userDto, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);

        }

        [Fact]
        public void Should_ReturnFalse_When_RequiredField_ConfirmEmail_IsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", "test@mail.com", null!, "Test@1234", "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidRegisterDto(userDto, errorContext, out _);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public void Should_RefOut_ValidationError_When_RequiredField_ConfirmEmail_IsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", "test@mail.com", null!, "Test@1234", "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            userDtoValidator.IsValidRegisterDto(userDto, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);

        }
        #endregion

        #region Email Validation Tests

        [Fact]
        public void Should_ReturnFalse_When_RequiredField_Email_IsEmpty()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", string.Empty, "test@mail.com", "Test@1234", "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidRegisterDto(userDto, errorContext, out _);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public void Should_RefOut_ValidationError_When_RequiredField_Email_IsEmpty()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", string.Empty, "test@mail.com", "Test@1234", "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            userDtoValidator.IsValidRegisterDto(userDto, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);

        }

        [Fact]
        public void Should_ReturnFalse_When_RequiredField_Email_IsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", null!, "test@mail.com", "Test@1234", "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidRegisterDto(userDto, errorContext, out _);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public void Should_RefOut_ValidationError_When_RequiredField_Email_IsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", null!, "test@mail.com", "Test@1234", "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            userDtoValidator.IsValidRegisterDto(userDto, errorContext, out var validationError);

            // Assert
            Assert.NotNull(validationError);

        }
        #endregion

        #region UserName Validation Tests
        [Fact]
        public void Should_ReturnFalse_When_RequiredField_UserName_IsEmpty()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new(string.Empty, "test@mail.com", "test@mail.com", "Test@1234", "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidRegisterDto(userDto, errorContext, out _);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public void Should_RefOut_NoValidationErrors_When_RequiredField_UserName_IsNotEmpty()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new("Testuser", "test@mail.com", "test@mail.com", "Test@1234", "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            userDtoValidator.IsValidRegisterDto(userDto, errorContext, out var validationErrors);

            // Assert
            Assert.Empty(validationErrors);
        }

        [Fact]
        public void Should_RefOut_ValidationError_When_RequiredField_UserName_IsEmpty()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new(string.Empty, "test@mail.com", "test@mail.com", "Test@1234", "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            userDtoValidator.IsValidRegisterDto(userDto, errorContext, out var validationErrors);

            // Assert
            Assert.NotNull(validationErrors);
            Assert.NotEmpty(validationErrors);
        }

        [Fact]
        public void Should_ReturnFalse_When_RequiredField_UserName_IsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new(null!, "test@mail.com", "test@mail.com", "Test@1234", "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            var result = userDtoValidator.IsValidRegisterDto(userDto, errorContext, out _);

            // Assert
            Assert.False(result);

        }

        [Fact]
        public void Should_RefOut_ValidationError_When_RequiredField_UserName_IsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto userDto = new(null!, "test@mail.com", "test@mail.com", "Test@1234", "Test@1234");

            var errorContext = DefineErrorContext();

            // Act
            userDtoValidator.IsValidRegisterDto(userDto, errorContext, out var validationErrors);

            // Assert
            Assert.NotEmpty(validationErrors);

        }
        #endregion

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

        private ErrorContext DefineErrorContext()
        {
            return new ErrorContext(
                layer: "Service",
                serviceName: "AuthService",
                methodName: "RegisterUserAsync",
                operation: OperationType.Create,
                entityName: "User",
                fieldName: null,
                confirmFieldName: null);
        }
    }
}
