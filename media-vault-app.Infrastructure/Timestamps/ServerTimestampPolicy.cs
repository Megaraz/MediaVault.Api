using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Infrastructure.Timestamps;

public sealed class ServerTimestampPolicy(TimeProvider timeProvider)
{
    public void Initialize<TKey>(IEntity<TKey> entity)
        where TKey : notnull, IEquatable<TKey>
    {
        var now = GetUtcNow();
        entity.CreatedAtUtc = now;
        entity.UpdatedAtUtc = now;
    }

    public void ApplyUpdate<TKey>(
        IEntity<TKey> entity,
        DateTime originalCreatedAtUtc,
        DateTime originalUpdatedAtUtc,
        bool hasMeaningfulChanges)
        where TKey : notnull, IEquatable<TKey>
    {
        entity.CreatedAtUtc = originalCreatedAtUtc;
        entity.UpdatedAtUtc = hasMeaningfulChanges
            ? NextUpdateTimestamp(originalUpdatedAtUtc)
            : originalUpdatedAtUtc;
    }

    private DateTime GetUtcNow() => timeProvider.GetUtcNow().UtcDateTime;

    private DateTime NextUpdateTimestamp(DateTime previousUpdatedAtUtc)
    {
        var now = GetUtcNow();
        return now > previousUpdatedAtUtc
            ? now
            : previousUpdatedAtUtc == DateTime.MaxValue
                ? previousUpdatedAtUtc
                : previousUpdatedAtUtc.AddTicks(1);
    }
}
