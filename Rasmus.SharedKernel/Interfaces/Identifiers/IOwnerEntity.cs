namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IOwnerEntity<TKeyOwner>
        : IEntity<TKeyOwner>, IConcurrencyVersion
            where TKeyOwner : notnull, IEquatable<TKeyOwner>
    {
    }
}
