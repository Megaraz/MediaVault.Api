using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using media_vault_app.Application.Results;
using media_vault_app.Application.Validation;

namespace media_vault_app.Tests.Validation;

public class MediaVaultResultPolicyTests
{
    [Theory]
    [InlineData("", "A value for the entity 'User' is required and cannot be null or empty.")]
    [InlineData("Email", "A value for the field 'Email' is required and cannot be null or empty.")]
    public void RequiredValidation_PreservesSafeMessage(string fieldName, string expectedMessage)
    {
        var context = new ErrorContext(
            OperationType.Create,
            "User",
            string.IsNullOrEmpty(fieldName) ? null : fieldName);

        var error = MediaVaultValidationError.Required(context);

        Assert.Equal(expectedMessage, error.UserMessage);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal(ValidationErrorType.Required, error.ValidationErrorType);
    }

    [Fact]
    public void NonMatchingValidation_PreservesMessageAndConfirmationField()
    {
        var context = new ErrorContext(OperationType.Create, "User", "Password");

        var error = MediaVaultValidationError.NonMatchingValues(context, "ConfirmPassword");

        Assert.Equal("The values for 'Password' and 'ConfirmPassword' do not match.", error.UserMessage);
        Assert.Equal("ConfirmPassword", error.FieldName);
    }

    [Fact]
    public void IdentifierValidation_UsesNeutralMediaVaultPolicy()
    {
        var context = new ErrorContext(OperationType.Get, "MediaEntry", "id");

        var invalid = Guid.Empty.IsNotValidMediaVaultId(context, out var error);

        Assert.True(invalid);
        Assert.Equal("A value for the field 'id' is required and cannot be null or empty.", error.UserMessage);
    }

    [Fact]
    public void RequiredFieldAggregation_PreservesInputOrderAndSafeMessages()
    {
        var context = new ErrorContext(OperationType.Create, "User");
        (string FieldName, string? Value)[] values =
        [
            ("Username", null),
            ("Email", ""),
            ("Password", "present")
        ];

        var invalid = values.HasMissingRequiredFields(context, out var errors);

        Assert.True(invalid);
        Assert.Equal(["Username", "Email"], errors.Select(error => error.FieldName));
        Assert.All(errors, error => Assert.False(string.IsNullOrWhiteSpace(error.UserMessage)));
    }

    [Fact]
    public void CoreFactories_PreserveStableResultMessages()
    {
        var context = new ErrorContext(OperationType.Get, "MediaEntry");

        var notFound = Result.Failure(MediaVaultErrors.NotFound(context));
        var validation = Result.ValidationFailure(
            [MediaVaultValidationError.Required(context)],
            MediaVaultResultMessages.ValidationFailure);

        Assert.Equal("MediaEntry not found", notFound.Message);
        Assert.Equal(MediaVaultResultMessages.ValidationFailure, validation.Message);
    }
}
