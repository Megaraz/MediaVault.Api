using Megaraz.ResultPattern;

namespace Rasmus.SharedKernel.ResultPatternCompatibility;

/// <summary>
/// Shared formatting for the temporary database and HTTP adapters used while later migration
/// issues replace those extension types. Remove this bridge with the legacy implementation in #95.
/// </summary>
internal static class TemporaryResultPatternBridge
{
    public static string FormatDescription(ErrorContext context, string detail)
    {
        ArgumentNullException.ThrowIfNull(context);
        return $"An error occurred during {context.Operation} on entity {context.EntityName}: {detail}";
    }
}
