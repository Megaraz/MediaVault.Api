using System;
using System.Collections.Generic;
namespace Rasmus.SharedKernel.ResultPattern
{
    public record ErrorContext(
        string Layer,
        string ServiceName,
        string MethodName,
        OperationType Operation,
        string EntityName,
        string? FieldName = null,
        string? ConfirmFieldName = null)
    {

        public string DescriptionPrefix { get; init; } = FormatDescriptionPrefix(Layer, ServiceName, MethodName, Operation, EntityName);
        public string FullDescription => $"{DescriptionPrefix}{FormatDescriptionSuffix(DescriptionSuffix)}";
        public string? DescriptionSuffix { get; init; } = null;

        private static string FormatDescriptionSuffix(string? DescriptionSuffix)
        {
            return $"{Environment.NewLine}Reason: " +
                   $"{(string.IsNullOrEmpty(DescriptionSuffix) 
                        ? "Unknown or unspecified" 
                        : DescriptionSuffix)}";
        }
        private static string FormatDescriptionPrefix(
            string layer,
            string serviceName,
            string methodName,
            OperationType operation,
            string entityName)
        {
            return $"An error occurred during {operation} on entity {entityName}: {Environment.NewLine}" +
                   $"Layer: {layer}{Environment.NewLine}" +
                   $"Service: {serviceName}{Environment.NewLine}" +
                   $"Method: {methodName}";
        }
    }
}
