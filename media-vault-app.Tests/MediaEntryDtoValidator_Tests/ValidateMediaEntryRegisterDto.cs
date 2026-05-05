using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.Validators.MediaEntry;
using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.ResultPattern;
using Xunit.Abstractions;

namespace media_vault_app.Tests.MediaEntryDtoValidator_Tests
{
    public class ValidateMediaEntryRegisterDto
    {
        private readonly ITestOutputHelper _output;

        public ValidateMediaEntryRegisterDto(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void IsValidRegisterDto_Should_ReturnTrue_And_NoErrors_When_AllFieldsAreValid()
        {
            // Arrange
            var mediaEntryDtoValidator = new MediaEntryDtoValidator();
            var errorContext = DefineErrorContext();

            var mediaEntryDto = new MovieEntryCreateDto
            {
                IdExternal = null,
                Status = Status.Completed,
                Title = "Test Media Entry",
                Rating = 4.5m,
                Review = "Great media entry!",
                Genres = null,
                ReleaseDate = null,
                ImageUrl = null,
                RuntimeMinutes = 120,
                Overview = "This is a test media entry used for validating the MediaEntryDtoValidator's IsValidCreateDto method."
            };

            // Act
            var result = mediaEntryDtoValidator.IsValidCreateDto(mediaEntryDto, errorContext, out var errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void IsValidRegisterDto_Should_ReturnFalse_And_Errors_When_MediaEntryRegisterDtoIsNull()
        {
            // Arrange
            var mediaEntryDtoValidator = new MediaEntryDtoValidator();
            var errorContext = DefineErrorContext();
            MediaEntryCreateDto? mediaEntryDto = null;

            // Act
            var result = mediaEntryDtoValidator.IsValidCreateDto(mediaEntryDto!, errorContext, out var errors);

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
        public void IsValidRegisterDto_Should_ReturnFalse_And_Errors_When_Title_IsNullOrWhiteSpace(string? value)
        {
            // Arrange
            var mediaEntryDtoValidator = new MediaEntryDtoValidator();
            var errorContext = DefineErrorContext();

            var mediaEntryDto = new MovieEntryCreateDto
            {
                IdExternal = null,
                Status = Status.Completed,
                Title = value!,
                Rating = 4.5m,
                Review = "Great media entry!",
                Genres = null,
                ReleaseDate = null,
                ImageUrl = null,
                RuntimeMinutes = 120
            };

            // Act
            var result = mediaEntryDtoValidator.IsValidCreateDto(mediaEntryDto, errorContext, out var errors);

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
            return new ErrorContext
            (
                Layer: "Application",
                ServiceName: "MediaEntryWriteService",
                MethodName: "CreateMediaEntryAsync",
                Operation: OperationType.Create,
                EntityName: "MediaEntry",
                FieldName: fieldName
            );

        }
    }
}
