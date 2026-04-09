using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Validators.User;
using Rasmus.SharedKernel.ResultPattern;
using Xunit.Abstractions;

namespace media_vault_app.Tests.UserDtoValidator_Tests
{
    public class UserDtoValidatorTests
    {
        private readonly ITestOutputHelper _output;

        public UserDtoValidatorTests(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void ValidateUserRegisterDto_Should_ReturnValidationError_When_UserRegisterDtoIsNull()
        {
            // Arrange
            var userDtoValidator = new UserDtoValidator();

            UserRegisterDto? userDto = null;

            var dtoValidationErrorContext = DefineErrorContext("RegisterUserAsync", OperationType.Create);

            // Act
            userDtoValidator.IsValidRegisterDto(userDto, dtoValidationErrorContext, out var validationErrors);

            // Assert
            Assert.NotNull(validationErrors);
            Assert.True(validationErrors.Any());
            Assert.True(validationErrors.All(e => e.Type == ErrorType.Validation));
            Assert.True(validationErrors.All(e => e.ValidationErrorType == ValidationErrorType.Required));
            Assert.True(validationErrors.All(e => e.Code == "Create.User.Required"));
            Assert.True(validationErrors.All(e => !string.IsNullOrWhiteSpace(e.Description)));

            _output.WriteLine("Validation Errors:" + $" {string.Join(", ", validationErrors.Select(e => e.Description))}");
        }


        private ErrorContext DefineErrorContext(string methodName, OperationType operation, string? fieldName = null, string? confirmFieldName = null)
        {
            return new ErrorContext(
                layer: "Service",
                serviceName: "AuthService",
                methodName: methodName,
                operation: operation,
                entityName: "User",
                fieldName: fieldName,
                confirmFieldName: confirmFieldName);
        }
    }
}
