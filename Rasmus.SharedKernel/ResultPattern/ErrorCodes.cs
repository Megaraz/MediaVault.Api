using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.ResultPattern
{
    public static class ErrorCodes
    {
        public static class Operation
        {
            public const string Create = "Create";
            public const string Update = "Update";
            public const string Delete = "Delete";
            public const string Get = "Get";
            public const string GetCollection = "GetCollection";
        }

        public static class ValidationError
        {
            public const string Required = "Required";
            public const string InvalidFormat = "InvalidFormat";
            public const string TooShort = "TooShort";
            public const string TooLong = "TooLong";
            public const string OutOfRange = "OutOfRange";

        }

        public static class DatabaseError
        {
            public const string DbFailure = "DbFailure";
        }


        public static class GeneralError
        {
            public const string InvalidInput = "InvalidInput";
            public const string NotFound = "NotFound";
            public const string Conflict = "Conflict";
            public const string Unauthorized = "Unauthorized";
            public const string Forbidden = "Forbidden";
            public const string Failure = "Failure";
        }
    }
}
