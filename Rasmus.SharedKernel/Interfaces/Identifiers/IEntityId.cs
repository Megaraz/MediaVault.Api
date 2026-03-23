namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IEntityId<TKey>
    {
        TKey Id { get; set; }
    }
}
