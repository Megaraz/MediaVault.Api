using Megaraz.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services.CrudServiceInterfaces
{
    public interface IGetByIdService<TKey, TDetailedDto>
        where TDetailedDto : notnull
    {
        Task<Result<TDetailedDto>> GetByIdAsync(TKey id, CancellationToken ct = default);
    }
}
