using Megaraz.ResultPattern;
using Megaraz.ResultPattern.AspNetCore;
using Rasmus.SharedKernel.Interfaces.ErrorLogger;

namespace media_vault_app.Infrastructure.Diagnostics;

public sealed class ErrorLogPolicy : IErrorLogPolicy
{
    public bool ShouldLog(Error error) => error switch
    {
        ValidationError => false,
        HttpError httpError => httpError.HttpErrorType is not (HttpErrorType.BadRequest or HttpErrorType.NotFound or HttpErrorType.Conflict or HttpErrorType.UnprocessableContent),
        _ when error.Type == ErrorType.Cancelled => false,
        _ => true
    };
}
