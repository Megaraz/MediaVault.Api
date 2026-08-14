using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace media_vault_app.Infrastructure.API.Clients;

public static class ProviderResilienceNames
{
    public const string RootSectionName = "RequestResilience:Providers";
    public const string Rawg = "Rawg";
    public const string Tmdb = "Tmdb";
    public const string GoogleBooks = "GoogleBooks";

    public static IReadOnlyList<string> All { get; } = [Rawg, Tmdb, GoogleBooks];

    public static string GetSectionName(string provider) =>
        $"{RootSectionName}:{provider}";
}

public sealed class ProviderResilienceOptions
{
    [Range(1, 600_000)]
    public int AttemptTimeoutMilliseconds { get; set; }

    [Range(1, 600_000)]
    public int TotalTimeoutMilliseconds { get; set; }

    [Range(1, 1)]
    public int MaximumRetryAttempts { get; set; }

    [Range(1, 600_000)]
    public int BaseDelayMilliseconds { get; set; }

    [Range(1, 600_000)]
    public int MaximumDelayMilliseconds { get; set; }

    [Range(1, 600_000)]
    public int MaximumRetryAfterMilliseconds { get; set; }
}

public sealed class ProviderResilienceOptionsValidator(int enclosingRequestBudgetMilliseconds)
    : IValidateOptions<ProviderResilienceOptions>
{
    public ValidateOptionsResult Validate(string? name, ProviderResilienceOptions options)
    {
        var failures = new List<string>();

        if (options.AttemptTimeoutMilliseconds <= 0 ||
            options.TotalTimeoutMilliseconds <= 0 ||
            options.MaximumRetryAttempts is < 1 or > 1 ||
            options.BaseDelayMilliseconds <= 0 ||
            options.MaximumDelayMilliseconds <= 0 ||
            options.MaximumRetryAfterMilliseconds <= 0)
        {
            failures.Add("All provider resilience durations and the single retry count must be positive.");
        }

        if (options.AttemptTimeoutMilliseconds > options.TotalTimeoutMilliseconds)
            failures.Add("Attempt timeout must not exceed total timeout.");

        if (options.BaseDelayMilliseconds > options.MaximumDelayMilliseconds)
            failures.Add("Base delay must not exceed maximum delay.");

        var largestDelay = Math.Max(
            options.MaximumDelayMilliseconds,
            options.MaximumRetryAfterMilliseconds);
        var worstCaseMilliseconds =
            (long)options.AttemptTimeoutMilliseconds * (options.MaximumRetryAttempts + 1L) +
            largestDelay;

        if (worstCaseMilliseconds > options.TotalTimeoutMilliseconds)
        {
            failures.Add(
                "All attempts and the largest permitted retry delay must fit within total timeout.");
        }

        if (options.TotalTimeoutMilliseconds >= enclosingRequestBudgetMilliseconds)
        {
            failures.Add(
                "Provider total timeout must be shorter than the enclosing external-metadata request budget.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
