using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services.CrudServiceInterfaces
{
    public interface IUpdateService<TKey, TUpdateDto>
    {
        Task<Result> UpdateAsync(TKey id, TUpdateDto updateDto, CancellationToken ct = default);
    }
}
