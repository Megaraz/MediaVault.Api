using media_vault_app.Infrastructure.API.Contracts.GoogleBooks;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Clients
{
    public interface IGoogleBooksApiClient
    {
        Task<Result<GoogleBooksVolumeResponse>> GetBookAsync(string volumeId, CancellationToken cancellationToken = default);
        Task<Result<GoogleBooksSearchResponse>> SearchBooksAsync(
            List<string> queryParameters,
            CancellationToken cancellationToken = default);
    }
}
