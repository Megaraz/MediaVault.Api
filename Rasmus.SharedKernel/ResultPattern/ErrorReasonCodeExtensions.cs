using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.ResultPattern
{
    public static class ErrorReasonCodeExtensions
    {
        // Extension method to convert ErrorReasonCode enum values to their corresponding string representations for use in error codes.
        // This makes the error codes more readable and consistent when included in the final error code string.
        public static string ToCodePart(this ErrorReasonCode reason) => reason switch
        {
            ErrorReasonCode.Custom => "Custom",

            ErrorReasonCode.ValidationRequired => "Required",
            ErrorReasonCode.ValidationInvalidFormat => "InvalidFormat",
            ErrorReasonCode.ValidationOutOfRange => "OutOfRange",
            ErrorReasonCode.ValidationNonMatchingValues => "NonMatchingValues",
            ErrorReasonCode.ValidationTooShort => "TooShort",
            ErrorReasonCode.ValidationTooLong => "TooLong",

            ErrorReasonCode.DatabaseFailure => "DbFailure",

            ErrorReasonCode.GeneralFailure => "Failure",
            ErrorReasonCode.GeneralNotFound => "NotFound",
            ErrorReasonCode.GeneralConflict => "Conflict",
            ErrorReasonCode.GeneralUnauthorized => "Unauthorized",
            ErrorReasonCode.GeneralForbidden => "Forbidden",

            ErrorReasonCode.UserLoginFailure => "LoginFailure",

            _ => "Unknown"
        };
    }
}
