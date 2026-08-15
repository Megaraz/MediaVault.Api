using System.ComponentModel.DataAnnotations;

namespace media_vault_app.API.RateLimiting;

public sealed class RateLimitingOptions : IValidatableObject
{
    public const string SectionName = "RateLimiting";

    [Required]
    public FixedWindowRateLimitOptions LoginByIp { get; init; } = new();

    [Required]
    public FixedWindowRateLimitOptions RegistrationByIp { get; init; } = new();

    [Required]
    public FixedWindowRateLimitOptions RawgMetadataByUser { get; init; } = new();

    [Required]
    public TokenBucketRateLimitOptions TmdbMetadataByUser { get; init; } = new();

    [Required]
    public FixedWindowRateLimitOptions GoogleBooksMetadataByUser { get; init; } = new();

    [Required]
    public FixedWindowRateLimitOptions AuthenticatedWriteByUser { get; init; } = new()
    {
        PermitLimit = 30,
        WindowSeconds = 60,
        QueueLimit = 0
    };

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in Validate("LoginByIp", LoginByIp)
            .Concat(Validate("RegistrationByIp", RegistrationByIp))
            .Concat(Validate("RawgMetadataByUser", RawgMetadataByUser))
            .Concat(Validate("TmdbMetadataByUser", TmdbMetadataByUser))
            .Concat(Validate("GoogleBooksMetadataByUser", GoogleBooksMetadataByUser))
            .Concat(Validate("AuthenticatedWriteByUser", AuthenticatedWriteByUser)))
        {
            yield return result;
        }
    }

    private static IEnumerable<ValidationResult> Validate(string name, IValidatableObject options) =>
        options.Validate(new ValidationContext(options))
            .Select(result => new ValidationResult(result.ErrorMessage, result.MemberNames.Select(member => $"{name}.{member}")));
}

public sealed class FixedWindowRateLimitOptions : IValidatableObject
{
    public int PermitLimit { get; init; }
    public int WindowSeconds { get; init; }
    public int QueueLimit { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PermitLimit <= 0)
            yield return new ValidationResult("PermitLimit must be positive.", [nameof(PermitLimit)]);
        if (WindowSeconds <= 0)
            yield return new ValidationResult("WindowSeconds must be positive.", [nameof(WindowSeconds)]);
        if (QueueLimit != 0)
            yield return new ValidationResult("QueueLimit must be zero.", [nameof(QueueLimit)]);
    }
}

public sealed class TokenBucketRateLimitOptions : IValidatableObject
{
    public int TokenLimit { get; init; }
    public int TokensPerPeriod { get; init; }
    public int ReplenishmentPeriodSeconds { get; init; }
    public int QueueLimit { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TokenLimit <= 0)
            yield return new ValidationResult("TokenLimit must be positive.", [nameof(TokenLimit)]);
        if (TokensPerPeriod <= 0)
            yield return new ValidationResult("TokensPerPeriod must be positive.", [nameof(TokensPerPeriod)]);
        if (ReplenishmentPeriodSeconds <= 0)
            yield return new ValidationResult("ReplenishmentPeriodSeconds must be positive.", [nameof(ReplenishmentPeriodSeconds)]);
        if (QueueLimit != 0)
            yield return new ValidationResult("QueueLimit must be zero.", [nameof(QueueLimit)]);
    }
}
