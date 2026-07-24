using Rasmus.SharedKernel.Interfaces.ErrorLogger;

namespace Rasmus.SharedKernel.ResultPattern
{
    public sealed class ErrorLogPolicy : IErrorLogPolicy
    {
        public bool ShouldLog(Error error)
        {
            return error switch
            {
                ValidationError => false,
                DatabaseError => true,
                HttpError httpError => ShouldLogHttpError(httpError),

                _ when error.Type == ErrorType.Cancelled => false,

                _ => true
            };
        }

        private static bool ShouldLogHttpError(HttpError error)
        {
            return error.HttpErrorType switch
            {
                HttpErrorType.BadRequest => false,
                HttpErrorType.NotFound => false,
                HttpErrorType.Conflict => false,
                HttpErrorType.UnprocessableContent => false,

                HttpErrorType.Unauthorized => true,
                HttpErrorType.Forbidden => true,
                HttpErrorType.InternalServerError => true,
                HttpErrorType.TooManyRequests => true,
                HttpErrorType.TransportFailure => true,
                HttpErrorType.MalformedResponse => true,
                HttpErrorType.UnexpectedStatusCode => true,
                HttpErrorType.Custom => true,

                _ => true
            };
        }
    }
}
