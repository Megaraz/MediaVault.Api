using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services
{
    public interface ISearchService<TSearchResultDto>
    {
        Task<Result<IReadOnlyList<TSearchResultDto>>> SearchAsync(string searchTerm, int page = 1, CancellationToken ct = default);
    }
}
