using System.ComponentModel.DataAnnotations;

namespace media_vault_app.API.Diagnostics;

public sealed class RequestBudgetOptions
{
    public const string SectionName = "RequestTimeouts";

    [Range(1, 600_000)]
    public int AuthenticationMilliseconds { get; init; } = 15_000;

    [Range(1, 600_000)]
    public int ExternalMetadataMilliseconds { get; init; } = 20_000;
}
