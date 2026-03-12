namespace Rasmus.SharedKernel.Interfaces
{
    public interface IEntityId<TKey>
    {
        TKey Id { get; set; }
    }
}
