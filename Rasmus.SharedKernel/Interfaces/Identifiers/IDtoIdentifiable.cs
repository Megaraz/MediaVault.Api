namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IDtoIdentifiable<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        TKey Id { get; init; }
    }
}
