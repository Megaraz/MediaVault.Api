using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests;

internal static class ValidationErrorAssert
{
    public static void IsRequired(ValidationError error, string? fieldName = null, string? entityName = null)
    {
        AssertCommon(error, ValidationErrorType.Required);
        AssertContainsIfProvided(error.Description, fieldName);
        AssertContainsIfProvided(error.Description, entityName);
    }

    public static void IsOutOfRange(ValidationError error, string? fieldName = null, string? entityName = null)
    {
        AssertCommon(error, ValidationErrorType.OutOfRange);
        AssertContainsIfProvided(error.Description, fieldName);
        AssertContainsIfProvided(error.Description, entityName);
    }

    public static void IsNonMatching(ValidationError error, string? fieldName = null, string? confirmFieldName = null, string? entityName = null)
    {
        AssertCommon(error, ValidationErrorType.NonMatchingValues);
        AssertContainsIfProvided(error.Description, fieldName);
        AssertContainsIfProvided(error.Description, confirmFieldName);
        AssertContainsIfProvided(error.Description, entityName);
    }

    private static void AssertCommon(ValidationError error, ValidationErrorType expectedType)
    {
        Assert.NotNull(error);
        Assert.False(string.IsNullOrWhiteSpace(error.Code));
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.False(string.IsNullOrWhiteSpace(error.Description));
        Assert.Equal(expectedType, error.ValidationErrorType);
    }

    private static void AssertContainsIfProvided(string value, string? expectedSubstring)
    {
        if (!string.IsNullOrWhiteSpace(expectedSubstring))
        {
            Assert.Contains(expectedSubstring, value, StringComparison.OrdinalIgnoreCase);
        }
    }
}
