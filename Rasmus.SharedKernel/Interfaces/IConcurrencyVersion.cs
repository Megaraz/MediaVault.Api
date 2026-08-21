namespace Rasmus.SharedKernel.Interfaces;

/// <summary>
/// Exposes a server-owned optimistic-concurrency version.
/// </summary>
public interface IConcurrencyVersion
{
    int Version { get; set; }
}
