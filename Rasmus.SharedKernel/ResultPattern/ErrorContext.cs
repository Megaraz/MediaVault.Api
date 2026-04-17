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
        private string? _descriptionSuffix;
        public string? DescriptionSuffix
        {
            get { return _descriptionSuffix; }
            init { _descriptionSuffix = value is not null ? $"{Environment.NewLine}Reason: {value}" : null; }
        }
        public string DescriptionPrefix { get; init; } = FormatDescriptionPrefix(Layer, ServiceName, MethodName, Operation, EntityName);
        public string FullDescription => $"{DescriptionPrefix}{DescriptionSuffix}";
        private static string FormatDescriptionPrefix(string layer, string serviceName, string methodName, OperationType operation, string entityName)
        {
            return $"An error occurred during {operation} on entity {entityName}: {Environment.NewLine}Layer: {layer}{Environment.NewLine}Service: {serviceName}{Environment.NewLine}Method: {methodName}";
        }
    }
}
