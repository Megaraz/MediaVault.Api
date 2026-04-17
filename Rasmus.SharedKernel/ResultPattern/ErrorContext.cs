using System;
using System.Collections.Generic;
namespace Rasmus.SharedKernel.ResultPattern
{
    public record ErrorContext
    {
        public string Layer { get; }
        public string ServiceName { get; }
        public string MethodName { get; }
        public OperationType Operation { get; }
        public string EntityName { get; set; }
        public string? ConfirmFieldName { get; set; }
        public string? FieldName { get; set; }
        //public string? DescriptionSuffix { get; set; }
        private string? _descriptionSuffix;

        public string? DescriptionSuffix
        {
            get { return _descriptionSuffix; }
            set { _descriptionSuffix = value is not null ? $"{Environment.NewLine}Reason: {value}" : null; }
        }

        public string DescriptionPrefix { get; init; }
        public string FullDescription => $"{DescriptionPrefix}{DescriptionSuffix}";

        public ErrorContext(string layer, string serviceName, string methodName, OperationType operation, string entityName, string? fieldName = null, string? confirmFieldName = null)
        {
            Layer = layer;
            ServiceName = serviceName;
            MethodName = methodName;
            Operation = operation;
            EntityName = entityName;
            FieldName = fieldName;
            ConfirmFieldName = confirmFieldName;
            DescriptionPrefix = FormatDescriptionPrefix(layer, serviceName, methodName, operation, entityName);
        }

        private static string FormatDescriptionPrefix(string layer, string serviceName, string methodName, OperationType operation, string entityName)
        {
            //return $"An error occurred in {layer} layer, in service {serviceName}, method {methodName}, during {operation} operation on entity {entityName}.";
            return $"An error occurred during {operation} on entity {entityName}: {Environment.NewLine}Layer: {layer}{Environment.NewLine}Service: {serviceName}{Environment.NewLine}Method: {methodName}";
        }


    }
}
