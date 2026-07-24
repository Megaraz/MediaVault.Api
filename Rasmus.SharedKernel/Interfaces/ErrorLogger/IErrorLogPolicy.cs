using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.ErrorLogger
{
    public interface IErrorLogPolicy
    {
        bool ShouldLog(Error error);
    }
}
