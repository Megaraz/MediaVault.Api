namespace Rasmus.SharedKernel.ResultPattern
{
    public static class ResultExtensions
    {
        public static Result<TOut> From<TIn, TOut>(this Result<TIn> result)
        {
            return new Result<TOut>(
                message: result.Message,
                validationErrors: result.ValidationErrors,
                primaryError: result.PrimaryError);
        }

        public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> map)
        {
            if (result.IsFailure)
                return result.From<TIn, TOut>();

            return Result<TOut>.Success(map(result.Value));
        }
    }
}
