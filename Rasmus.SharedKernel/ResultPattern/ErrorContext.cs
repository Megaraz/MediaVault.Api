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
        public string? FieldName { get; set; }
        public string? DescriptionSuffix { get; set; }
        public string DescriptionPrefix => $"An error occurred in {Layer} layer, in service {ServiceName}, method {MethodName}, during {Operation} operation on entity {EntityName}.";
        public ErrorContext(string layer, string serviceName, string methodName, OperationType operation, string entityName)
        {
            Layer = layer;
            ServiceName = serviceName;
            MethodName = methodName;
            Operation = operation;
            EntityName = entityName;
        }


    }
}
