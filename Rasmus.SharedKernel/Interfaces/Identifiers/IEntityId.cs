namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IEntityId<TKey> : ICreatedAtUtc
    {
        TKey Id { get; set; }
    }
}
