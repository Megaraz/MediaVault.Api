using media_vault_app.Application.DTOs.ExternalAPIs;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IGoogleBooksApiService
    {
        Task<Result<SearchResultDto>> GetBookByIdAsync(string volumeId, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<SearchResultDto>>> SearchBooksAsync(string search, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    }
}
