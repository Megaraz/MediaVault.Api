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

        [Fact]
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
        public void ValidationErrors_Should_BeEmpty_When_UserRegisterDtoIsNotNull_And_AllRequiredFieldsAreProvided()
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
        public void Should_OutRefValidationErrors_When_UserRegisterDtoIsNull()
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
        public void ValidationErrors_ErrorCode_Should_NotBeNullOrWhiteSpace_When_UserRegisterDtoIsNull()
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
        public void ValidationErrors_ErrorType_Should_NotBeNone_When_UserRegisterDtoIsNull()
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
        public void ValidationErrors_Description_Should_NotBeNullOrWhiteSpace_When_UserRegisterDtoIsNull()
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
