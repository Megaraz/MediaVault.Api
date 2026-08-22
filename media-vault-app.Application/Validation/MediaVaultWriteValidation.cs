using System.Net.Mail;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Validation;

public static class MediaVaultWriteValidationPolicy
{
    public const int UserNameMaxLength = 50;
    public const int EmailMaxLength = 254;
    public const int PasswordMaxLength = 128;

    public const int TitleMaxLength = 200;
    public const int ExternalIdMaxLength = 100;
    public const int OverviewMaxLength = 4_000;
    public const int ReviewMaxLength = 4_000;
    public const int AuthorMaxLength = 200;
    public const int AiringStatusMaxLength = 100;
    public const int UrlMaxLength = 2_048;
    public const int CollectionItemMaxLength = 50;
    public const int PcRequirementMaxLength = 2_000;

    public const int MaxGenres = 20;
    public const int MaxPlatforms = 20;
    public const int MaxSeasons = 100;

    public const decimal MinimumRating = 0m;
    public const decimal MaximumRating = 5m;
    public const decimal RatingStep = 0.5m;

    public const int MaximumRuntimeMinutes = 1_440;
    public const int MaximumHoursPlayed = 100_000;
    public const int MaximumMetacriticRating = 100;
    public const int MaximumSeasons = 1_000;
    public const int MaximumEpisodes = 100_000;
}

public static class MediaVaultWriteValidation
{
    public static void AddText(
        List<ValidationError> errors,
        string? value,
        ErrorContext errorContext,
        string fieldName,
        int maximumLength,
        bool required = false)
    {
        var fieldContext = errorContext with { FieldName = fieldName };

        if (required && value.IsMissingMediaVaultValue(fieldContext, out var requiredError))
        {
            errors.Add(requiredError);
            return;
        }

        if (string.IsNullOrWhiteSpace(value))
            return;

        if (value.Length > maximumLength)
        {
            errors.Add(MediaVaultValidationError.TooLong(
                fieldContext,
                $"<= {maximumLength} characters"));
        }

        if (value.Any(char.IsControl))
        {
            errors.Add(MediaVaultValidationError.InvalidFormat(
                fieldContext,
                "printable text"));
        }
    }

    public static void AddEmail(
        List<ValidationError> errors,
        string? value,
        ErrorContext errorContext,
        string fieldName,
        bool required = false)
    {
        AddText(
            errors,
            value,
            errorContext,
            fieldName,
            MediaVaultWriteValidationPolicy.EmailMaxLength,
            required);

        if (string.IsNullOrWhiteSpace(value))
            return;

        var candidate = value.Trim();
        var isValid = candidate.Count(character => character == '@') == 1
            && !candidate.Any(char.IsWhiteSpace)
            && MailAddress.TryCreate(candidate, out var address)
            && string.Equals(address.Address, candidate, StringComparison.OrdinalIgnoreCase);

        if (!isValid)
        {
            errors.Add(MediaVaultValidationError.InvalidFormat(
                errorContext with { FieldName = fieldName },
                "a valid email address"));
        }
    }

    public static void AddUrl(
        List<ValidationError> errors,
        string? value,
        ErrorContext errorContext,
        string fieldName)
    {
        AddText(
            errors,
            value,
            errorContext,
            fieldName,
            MediaVaultWriteValidationPolicy.UrlMaxLength);

        if (string.IsNullOrWhiteSpace(value))
            return;

        var candidate = value.Trim();
        var isValid = !candidate.Any(char.IsWhiteSpace)
            && Uri.TryCreate(candidate, UriKind.Absolute, out var uri)
            && uri is not null
            && (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            && !string.IsNullOrWhiteSpace(uri.Host);

        if (!isValid)
        {
            errors.Add(MediaVaultValidationError.InvalidFormat(
                errorContext with { FieldName = fieldName },
                "an absolute HTTP or HTTPS URL"));
        }
    }

    public static void AddEnum<TEnum>(
        List<ValidationError> errors,
        TEnum value,
        ErrorContext errorContext,
        string fieldName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            errors.Add(MediaVaultValidationError.OutOfRange(
                errorContext with { FieldName = fieldName },
                "a defined enum value"));
        }
    }

    public static void AddIntegerRange(
        List<ValidationError> errors,
        int value,
        ErrorContext errorContext,
        string fieldName,
        int minimum,
        int maximum)
    {
        if (value < minimum || value > maximum)
        {
            errors.Add(MediaVaultValidationError.OutOfRange(
                errorContext with { FieldName = fieldName },
                $"{minimum} to {maximum}"));
        }
    }

    public static void AddDecimalRange(
        List<ValidationError> errors,
        decimal value,
        ErrorContext errorContext,
        string fieldName,
        decimal minimum,
        decimal maximum,
        decimal? step = null)
    {
        var hasInvalidStep = step is { } increment && (value - minimum) % increment != 0;
        if (value < minimum || value > maximum || hasInvalidStep)
        {
            var range = step is { } valueStep
                ? $"{minimum} to {maximum} in {valueStep} increments"
                : $"{minimum} to {maximum}";
            errors.Add(MediaVaultValidationError.OutOfRange(
                errorContext with { FieldName = fieldName },
                range));
        }
    }

    public static void AddRating(
        List<ValidationError> errors,
        decimal value,
        ErrorContext errorContext,
        string fieldName)
    {
        AddDecimalRange(
            errors,
            value,
            errorContext,
            fieldName,
            MediaVaultWriteValidationPolicy.MinimumRating,
            MediaVaultWriteValidationPolicy.MaximumRating,
            MediaVaultWriteValidationPolicy.RatingStep);
    }

    public static void AddStringCollection(
        List<ValidationError> errors,
        IEnumerable<string?>? values,
        ErrorContext errorContext,
        string fieldName,
        int maximumCount)
    {
        var items = MaterializeCollection(errors, values, errorContext, fieldName, maximumCount);
        if (items is null)
            return;

        for (var index = 0; index < items.Count; index++)
        {
            AddText(
                errors,
                items[index],
                errorContext,
                $"{fieldName}[{index}]",
                MediaVaultWriteValidationPolicy.CollectionItemMaxLength,
                required: true);
        }
    }

    public static IReadOnlyList<T>? MaterializeCollection<T>(
        List<ValidationError> errors,
        IEnumerable<T>? values,
        ErrorContext errorContext,
        string fieldName,
        int maximumCount)
    {
        if (values is null)
        {
            errors.Add(MediaVaultValidationError.Required(
                errorContext with { FieldName = fieldName }));
            return null;
        }

        var items = values.ToArray();
        if (items.Length > maximumCount)
        {
            errors.Add(MediaVaultValidationError.TooLong(
                errorContext with { FieldName = fieldName },
                $"<= {maximumCount} items"));
        }

        return items;
    }
}
