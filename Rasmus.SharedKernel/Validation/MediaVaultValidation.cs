using Megaraz.ResultPattern;

namespace Rasmus.SharedKernel.Validation;

/// <summary>MediaVault-owned validation factories that preserve stable client messages.</summary>
public static class MediaVaultValidationError
{
    public static ValidationError Required(ErrorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var message = string.IsNullOrWhiteSpace(context.FieldName)
            ? $"A value for the entity '{context.EntityName}' is required and cannot be null or empty."
            : $"A value for the field '{context.FieldName}' is required and cannot be null or empty.";
        return ValidationError.Required(context, userMessage: message);
    }

    public static ValidationError AlreadyExists(ErrorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var message = $"A {context.EntityName} with that {context.FieldName} already exists, please choose a different {context.FieldName}.";
        return ValidationError.AlreadyExists(context, userMessage: message);
    }

    public static ValidationError InvalidFormat(ErrorContext context, string expectedFormat)
    {
        ArgumentNullException.ThrowIfNull(context);
        var message = $"The field '{context.FieldName}' has an invalid format. Expected format: {expectedFormat}.";
        return ValidationError.InvalidFormat(context, expectedFormat, userMessage: message);
    }

    public static ValidationError OutOfRange(ErrorContext context, string range)
    {
        ArgumentNullException.ThrowIfNull(context);
        var message = $"The field '{context.FieldName}' is out of range. Expected range: {range}.";
        return ValidationError.OutOfRange(context, range, userMessage: message);
    }

    public static ValidationError TooShort(ErrorContext context, string range)
    {
        ArgumentNullException.ThrowIfNull(context);
        var message = $"The field '{context.FieldName}' is too short. Expected minimum length: {range}.";
        return ValidationError.TooShort(context, range, userMessage: message);
    }

    public static ValidationError TooLong(ErrorContext context, string range)
    {
        ArgumentNullException.ThrowIfNull(context);
        var message = $"The field '{context.FieldName}' is too long. Expected maximum length: {range}.";
        return ValidationError.TooLong(context, range, userMessage: message);
    }

    public static ValidationError NonMatchingValues(ErrorContext context, string? confirmFieldName = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        var message = !string.IsNullOrWhiteSpace(context.FieldName) && !string.IsNullOrWhiteSpace(confirmFieldName)
            ? $"The values for '{context.FieldName}' and '{confirmFieldName}' do not match."
            : "The provided values do not match.";
        return ValidationError.NonMatchingValues(context, confirmFieldName, userMessage: message);
    }

    public static ValidationError Custom(ErrorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        const string message = "A custom validation error occurred.";
        return ValidationError.Custom(context, description: message, userMessage: message);
    }
}

public static class MediaVaultValidator
{
    public static bool IsValidId<TKey>(TKey id)
    {
        if (id is null)
            return false;

        return !((id is string value && string.IsNullOrWhiteSpace(value)) ||
                 (id is Guid guid && guid == Guid.Empty) ||
                 (id is int integer && integer <= 0) ||
                 id.Equals(default(TKey)));
    }
}

/// <summary>
/// Validation extensions retained by MediaVault because the core package does not own identifier
/// or generic-null policy and its compatibility helpers intentionally use empty user messages.
/// Methods return <see langword="true"/> when the invalid condition is detected.
/// </summary>
public static class MediaVaultValidatorExtensions
{
    public static bool IsNotValidMediaVaultId<TKey>(
        this TKey id,
        ErrorContext errorContext,
        out ValidationError validationError)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        validationError = default!;

        if (MediaVaultValidator.IsValidId(id))
            return false;

        var fieldName = string.IsNullOrWhiteSpace(errorContext.FieldName) ? nameof(id) : errorContext.FieldName;
        validationError = MediaVaultValidationError.Required(errorContext with { FieldName = fieldName });
        return true;
    }

    public static bool IsMediaVaultNull<TValue>(
        this TValue? value,
        ErrorContext errorContext,
        out ValidationError validationError)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        validationError = default!;

        if (value is not null)
            return false;

        validationError = MediaVaultValidationError.Required(errorContext);
        return true;
    }

    public static bool HasMissingRequiredFields(
        this IEnumerable<(string FieldName, string? Value)> requiredValues,
        ErrorContext errorContext,
        out IReadOnlyList<ValidationError> validationErrors)
    {
        ArgumentNullException.ThrowIfNull(requiredValues);
        ArgumentNullException.ThrowIfNull(errorContext);

        var errors = new List<ValidationError>();
        foreach (var (fieldName, value) in requiredValues)
        {
            if (value.IsMissingMediaVaultValue(fieldName, errorContext, out var error))
                errors.Add(error);
        }

        validationErrors = errors;
        return errors.Count > 0;
    }

    public static bool IsMissingMediaVaultValue(
        this string? value,
        string fieldName,
        ErrorContext errorContext,
        out ValidationError validationError)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        validationError = default!;

        if (!string.IsNullOrWhiteSpace(value))
            return false;

        var resolvedFieldName = string.IsNullOrWhiteSpace(fieldName)
            ? errorContext.FieldName ?? nameof(value)
            : fieldName;
        validationError = MediaVaultValidationError.Required(errorContext with { FieldName = resolvedFieldName });
        return true;
    }

    public static bool IsMissingMediaVaultValue(
        this string? value,
        ErrorContext errorContext,
        out ValidationError validationError)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        validationError = default!;

        if (!string.IsNullOrWhiteSpace(value))
            return false;

        validationError = MediaVaultValidationError.Required(errorContext);
        return true;
    }

    public static bool IsBelowMediaVaultMinimum(
        this int value,
        int minimum,
        ErrorContext errorContext,
        out ValidationError validationError)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        validationError = default!;

        if (value >= minimum)
            return false;

        validationError = MediaVaultValidationError.OutOfRange(errorContext, $">= {minimum}");
        return true;
    }

    public static bool HasNonMatchingMediaVaultValues(
        this string value,
        string confirmValue,
        string fieldName,
        string confirmFieldName,
        ErrorContext errorContext,
        out ValidationError validationError)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        validationError = default!;

        if (value.IsMissingMediaVaultValue(fieldName, errorContext, out var valueError))
        {
            validationError = valueError;
            return true;
        }

        if (confirmValue.IsMissingMediaVaultValue(confirmFieldName, errorContext, out var confirmError))
        {
            validationError = confirmError;
            return true;
        }

        if (string.Equals(value, confirmValue, StringComparison.Ordinal))
            return false;

        validationError = MediaVaultValidationError.NonMatchingValues(
            errorContext with { FieldName = fieldName },
            confirmFieldName);
        return true;
    }
}
