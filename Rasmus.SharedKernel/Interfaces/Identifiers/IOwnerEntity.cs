namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IOwnerEntity<TKeyOwner>
        : IEntity<TKeyOwner>
            where TKeyOwner : notnull, IEquatable<TKeyOwner>
    {
    }
}
