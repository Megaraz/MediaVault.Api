using Megaraz.ResultPattern;

namespace media_vault_app.Application.Interfaces.Repos;

public interface IEntityExistsRepo<TKey>
    where TKey : notnull, IEquatable<TKey>
{
    Task<Result<bool>> ExistsAsync(TKey id, CancellationToken ct = default);
}
