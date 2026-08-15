using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.DTOs.Season;
using media_vault_app.Application.Validation;
using media_vault_app.Application.Validators.MediaEntry;
using media_vault_app.Domain.Enums;
using Megaraz.ResultPattern;
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
                ReleaseDate = default,
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
        public void IsValidRegisterDto_Should_RejectInvalidRatingAndStatus()
        {
            var validator = new MediaEntryDtoValidator();
            var dto = new MovieEntryCreateDto
            {
                Title = "Test Movie",
                Status = (Status)99,
                Rating = 4.25m
            };

            var result = validator.IsValidCreateDto(dto, DefineErrorContext(), out var errors);

            Assert.False(result);
            Assert.Equal(2, errors.Count(error => error.ValidationErrorType == ValidationErrorType.OutOfRange));
        }

        [Fact]
        public void IsValidRegisterDto_Should_RejectUnsafeUrlAndOversizedCollection()
        {
            var validator = new MediaEntryDtoValidator();
            var dto = new MovieEntryCreateDto
            {
                Title = "Test Movie",
                ImageUrl = "javascript:alert(1)",
                Genres = Enumerable
                    .Repeat("genre", MediaVaultWriteValidationPolicy.MaxGenres + 1)
                    .ToList()
            };

            var result = validator.IsValidCreateDto(dto, DefineErrorContext(), out var errors);

            Assert.False(result);
            Assert.Contains(errors, error => error.ValidationErrorType == ValidationErrorType.InvalidFormat);
            Assert.Contains(errors, error => error.ValidationErrorType == ValidationErrorType.TooLong);
        }

        [Fact]
        public void IsValidRegisterDto_Should_RejectInvalidNestedGameRequirements()
        {
            var validator = new MediaEntryDtoValidator();
            var dto = new GameEntryCreateDto
            {
                Title = "Test Game",
                PcRequirements = new GamePcRequirementsDto(
                    new string('x', MediaVaultWriteValidationPolicy.PcRequirementMaxLength + 1),
                    null,
                    null,
                    null,
                    null)
            };

            var result = validator.IsValidCreateDto(dto, DefineErrorContext(), out var errors);

            Assert.False(result);
            Assert.Contains(
                errors,
                error => error.FieldName?.Contains("PcRequirements.Minimum", StringComparison.Ordinal) == true &&
                          error.ValidationErrorType == ValidationErrorType.TooLong);
        }

        [Fact]
        public void IsValidRegisterDto_Should_RejectInvalidNestedSeason()
        {
            var validator = new MediaEntryDtoValidator();
            var dto = new TvSeriesEntryCreateDto
            {
                Title = "Test Series",
                Seasons =
                [
                    new SeasonCreateDto
                    {
                        Status = (Status)99,
                        Rating = 4.25m,
                        ImageUrl = "not-a-url"
                    }
                ]
            };

            var result = validator.IsValidCreateDto(dto, DefineErrorContext(), out var errors);

            Assert.False(result);
            Assert.True(errors.Count(error =>
                error.FieldName?.StartsWith("Seasons[0].", StringComparison.Ordinal) == true) >= 3);
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
                ReleaseDate = default,
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

            _output.WriteLine($"Code: {requiredError.Code}");
            _output.WriteLine(requiredError.Description);

        }


        private ErrorContext DefineErrorContext(string? fieldName = null)
        {
            return new ErrorContext
            (
                operation: OperationType.Create,
                entityName: "MediaEntry",
                fieldName: fieldName
            );

        }
    }
}
