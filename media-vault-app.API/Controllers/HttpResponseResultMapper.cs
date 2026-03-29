using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.API.Controllers
{
    public static class HttpResponseResultMapper
    {
        //public static Result MapHttpResponseMessage(HttpResponseMessage? httpResponseMessage, ErrorContext errorContext)
        //{
        //    if (httpResponseMessage is null)
        //        return Result.Failure(Error.Unauthorized(errorContext), errorContext.DescriptionSuffix!);

        //    if (httpResponseMessage.IsSuccessStatusCode)
        //        return Result.Success();

        //    string? reason = httpResponseMessage.ReasonPhrase;
        //    //string statusMsg = $"API Error: {(int)httpResponseMessage.StatusCode} {reason}";

        //    //return httpResponseMessage.StatusCode switch
        //    //{
        //    //    HttpStatusCode.BadRequest => InvalidValue(statusMsg + (reason ?? "Bad Request")),
        //    //    (HttpStatusCode)422 => InvalidValue(statusMsg + (reason ?? "Unprocessable Entity")),
        //    //};

        //    }


    }
}
