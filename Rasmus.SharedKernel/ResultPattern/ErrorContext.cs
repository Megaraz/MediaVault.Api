using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.ResultPattern
{
    public class ErrorContext
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
            set { _descriptionSuffix = new StringBuilder(value).ToString(); } // TODO: Add "Reason: " prefix to description suffix and make sure it is included in the final error description when creating errors using this context
        }

        //public string DescriptionPrefix => $"An error occurred in {Layer} layer, in service {ServiceName}, method {MethodName}, during {Operation} operation on entity {EntityName}.";
        public string DescriptionPrefix { get; init; }
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
            return $"An error occurred during {operation} on entity {entityName}: {Environment.NewLine} Layer: {layer}{Environment.NewLine} Service: {serviceName}{Environment.NewLine} Method: {methodName}";
        }


    }
}
