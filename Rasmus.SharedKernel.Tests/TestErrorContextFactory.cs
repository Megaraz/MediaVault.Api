using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Tests;

internal static class TestErrorContextFactory
{
    public static ErrorContext Create(string? fieldName = null, string? confirmFieldName = null) =>
        new(
            Layer: "ResultPattern",
            ServiceName: "ValidatorExtensions",
            MethodName: "Test",
            Operation: OperationType.Create,
            EntityName: "User",
            FieldName: fieldName,
            ConfirmFieldName: confirmFieldName);
}
