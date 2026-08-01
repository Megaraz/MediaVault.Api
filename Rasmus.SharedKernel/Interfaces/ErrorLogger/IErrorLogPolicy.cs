using Megaraz.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.ErrorLogger
{
    public interface IErrorLogPolicy
    {
        bool ShouldLog(Error error);
    }
}
