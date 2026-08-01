using Megaraz.ResultPattern;

namespace Rasmus.SharedKernel.Tests;

internal static class PackageErrorContextFactory
{
    public static ErrorContext Create(string? fieldName = null) =>
        new(OperationType.Create, "User", fieldName);
}
