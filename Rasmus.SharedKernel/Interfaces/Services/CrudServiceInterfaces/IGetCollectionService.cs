using Megaraz.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services.CrudServiceInterfaces
{
    public interface IGetCollectionService<TDetailedDto, TMinimalDto>
    {
        Task<Result<IReadOnlyList<TDetailedDto>>> GetDetailedCollectionAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task<Result<IReadOnlyList<TMinimalDto>>> GetMinimalCollectionAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    }
}
