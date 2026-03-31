using media_vault_app.Application.DTOs.ExternalAPIs;
using media_vault_app.Domain.Enums;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface ITmdbApiService
    {
        Task<Result<SearchResultDto>> GetByIdAsync(int id, MediaEntryType mediaType, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<SearchResultDto>>> SearchAsync(string search, MediaEntryType mediaType, int page = 1, int pageSize = 10, string? ordering = null, CancellationToken cancellationToken = default);
    }
}
