using System.ComponentModel.DataAnnotations;

namespace media_vault_app.API.Configuration;

public sealed class CorsOptions : IValidatableObject
{
    public const string SectionName = "Cors";
    public const string PolicyName = "ConfiguredOrigins";

    public string[] AllowedOrigins { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AllowedOrigins is null || AllowedOrigins.Length == 0)
            yield break;

        for (var index = 0; index < AllowedOrigins.Length; index++)
        {
            var origin = AllowedOrigins[index];
            if (string.IsNullOrWhiteSpace(origin) ||
                origin.Any(char.IsWhiteSpace) ||
                origin.Contains('*') ||
                !Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
                uri is null ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
                string.IsNullOrWhiteSpace(uri.Host) ||
                !string.IsNullOrEmpty(uri.UserInfo) ||
                !string.IsNullOrEmpty(uri.Query) ||
                !string.IsNullOrEmpty(uri.Fragment) ||
                uri.AbsolutePath != "/" ||
                origin.EndsWith("/", StringComparison.Ordinal))
            {
                yield return new ValidationResult(
                    $"Cors:AllowedOrigins[{index}] must be a valid HTTP or HTTPS origin without a path, query, fragment, credentials, wildcard, whitespace, or trailing slash.",
                    [$"{nameof(AllowedOrigins)}[{index}]"]);
            }
        }
    }
}
