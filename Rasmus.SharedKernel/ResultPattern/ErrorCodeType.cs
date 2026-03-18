using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.ResultPattern
{
    public enum OperationType
    {
        Custom = 0,
        Create = 1,
        Update = 2,
        Delete = 3,
        Get = 4,
        GetCollection = 5,
    }

    //public enum ErrorCodeType
    //{
    //    Validation = 0,
    //    Database = 1,
    //    General = 2
    //}
    public static class ErrorCodeType
    {
        //public static class Operation
        //{
        //    public const string Create = "Create";
        //    public const string Update = "Update";
        //    public const string Delete = "Delete";
        //    public const string Get = "Get";
        //    public const string GetCollection = "GetCollection";
        //}

        public static class Custom
        {
            public const string CustomError = "CustomError";
        }


        public static class Validation
        {
            public const string Required = "Required";
            public const string InvalidFormat = "InvalidFormat";
            public const string NullValue = "NullValue";
            public const string OutOfRange = "OutOfRange";
            public const string TooShort = "TooShort";
            public const string TooLong = "TooLong";
            public const string Custom = "Custom";

        }

        public static class Database
        {
            public const string DbFailure = "DbFailure";
        }


        public static class General
        {
            public const string Failure = "Failure";
            public const string NotFound = "NotFound";
            public const string Conflict = "Conflict";
            public const string Unauthorized = "Unauthorized";
            public const string Forbidden = "Forbidden";
        }
    }
}
