namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IEntityId<TKey> : ICreatedAtUtc
        where TKey : notnull, IEquatable<TKey>
    {
        TKey Id { get; set; }
    }
}
