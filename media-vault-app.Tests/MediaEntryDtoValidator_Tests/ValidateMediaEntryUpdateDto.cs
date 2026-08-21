using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.Validators.MediaEntry;
using media_vault_app.Domain.Enums;
using Megaraz.ResultPattern;
using Xunit.Abstractions;

namespace media_vault_app.Tests.MediaEntryDtoValidator_Tests
{
    public class ValidateMediaEntryUpdateDto
    {

        private readonly ITestOutputHelper _output;

        public ValidateMediaEntryUpdateDto(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void IsValidUpdateDto_Should_ReturnFalse_And_Errors_When_MediaEntryUpdateDtoIsNull()
        {
            // Arrange
            var mediaEntryDtoValidator = new MediaEntryDtoValidator();
            var errorContext = DefineErrorContext();
            MediaEntryUpdateDto? mediaEntryDto = null;

            // Act
            var result = mediaEntryDtoValidator.IsValidUpdateDto(mediaEntryDto!, errorContext, out var errors);

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
        [Fact]
        public void IsValidUpdateDto_Should_ReturnTrue_And_NoErrors_When_AllFieldsAreValid()
        {
            // Arrange
            var mediaEntryDtoValidator = new MediaEntryDtoValidator();
            var errorContext = DefineErrorContext();

            var mediaEntryDto = new MovieEntryUpdateDto
            {
                ExpectedVersion = 1,
                IdExternal = null,
                Status = Status.Completed,
                Title = "Test Media Entry",
                Rating = 4.5m,
                Review = "Great media entry!",
                Genres = [],
                ReleaseDate = default,
                ImageUrl = null,
                RuntimeMinutes = 120
            };

            // Act
            var result = mediaEntryDtoValidator.IsValidUpdateDto(mediaEntryDto, errorContext, out var errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void IsValidUpdateDto_Should_RejectInvalidRating()
        {
            var validator = new MediaEntryDtoValidator();
            var dto = new MovieEntryUpdateDto
            {
                ExpectedVersion = 1,
                Title = "Test Movie",
                Rating = 5.1m,
                RuntimeMinutes = 120
            };

            var result = validator.IsValidUpdateDto(dto, DefineErrorContext(), out var errors);

            Assert.False(result);
            Assert.Contains(errors, error => error.ValidationErrorType == ValidationErrorType.OutOfRange);
        }

        [Fact]
        public void IsValidUpdateDto_Should_RejectMissingExpectedVersion()
        {
            var validator = new MediaEntryDtoValidator();
            var dto = new MovieEntryUpdateDto
            {
                Title = "Test Movie",
                Rating = 4m,
                RuntimeMinutes = 120
            };

            var result = validator.IsValidUpdateDto(dto, DefineErrorContext(), out var errors);

            Assert.False(result);
            var error = Assert.Single(errors);
            Assert.Equal(nameof(MediaEntryUpdateDto.ExpectedVersion), error.FieldName);
            Assert.Equal(ValidationErrorType.OutOfRange, error.ValidationErrorType);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void IsValidUpdateDto_Should_ReturnFalse_And_Errors_When_Title_IsNullOrWhiteSpace(string? value)
        {
            // Arrange
            var mediaEntryDtoValidator = new MediaEntryDtoValidator();
            var errorContext = DefineErrorContext();

            var mediaEntryDto = new MovieEntryUpdateDto
            {
                ExpectedVersion = 1,
                IdExternal = null,
                Status = Status.Completed,
                Title = value!,
                Rating = 4.5m,
                Review = "Great media entry!",
                Genres = [],
                ReleaseDate = default,
                ImageUrl = null,
                RuntimeMinutes = 120
            };

            // Act
            var result = mediaEntryDtoValidator.IsValidUpdateDto(mediaEntryDto, errorContext, out var errors);

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
                operation: OperationType.Update,
                entityName: "MediaEntry",
                fieldName: fieldName
            );

        }
    }

}
