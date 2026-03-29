using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.ResultPattern
{

    public enum HttpErrorType
    {
        Custom = 0,
        BadRequest = 1,
        Unauthorized = 2,
        Forbidden = 3,
        NotFound = 4,
        Conflict = 5,
        InternalServerError = 6,
    }

    public record HttpError : Error
    {
        public HttpError(string Code, string Description, ErrorType Type, Exception? exception = null) : base(Code, Description, Type, exception)
        {
        }
    }
}
