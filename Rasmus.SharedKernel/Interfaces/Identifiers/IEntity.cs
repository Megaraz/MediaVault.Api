namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IEntity<TKey> : ICreatedAtUtc, IUpdatedAtUtc
        where TKey : notnull, IEquatable<TKey>
    {
        TKey Id { get; set; }
    }
}
