namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IWriteableEntity<TKey> : ICreatedAtUtc, IUpdatedAtUtc
        where TKey : notnull, IEquatable<TKey>
    {
        TKey Id { get; set; }
    }
}
