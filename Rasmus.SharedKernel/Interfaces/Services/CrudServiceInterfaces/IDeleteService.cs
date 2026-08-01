using Megaraz.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services.CrudServiceInterfaces
{
    public interface IDeleteService<TKey>
    {
        Task<Result> DeleteAsync(TKey id, CancellationToken ct = default);
    }
}
