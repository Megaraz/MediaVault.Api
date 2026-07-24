using media_vault_app.Application.DTOs.GoogleBooks;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IGoogleBooksApiService
    {
        Task<Result<GoogleBooksDetailedDto>> GetBookByIdAsync(string volumeId, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<GoogleBooksDetailedDto>>> SearchBooksAsync(
            string search,
            int page = 1,
            int pageSize = 8,
            CancellationToken cancellationToken = default);
    }
}
