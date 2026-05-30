namespace Rasmus.SharedKernel.ResultPattern
{
    public record ErrorContext(
        string Layer,
        string ServiceName,
        string MethodName,
        OperationType Operation,
        string EntityName,
        string? FieldName = null)
    {
    }
}
