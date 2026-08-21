namespace Rasmus.SharedKernel.Interfaces.Identifiers
{
    public interface IDependentEntity<TKeyOwner, TKeyDependent>
        : IEntity<TKeyDependent>, IConcurrencyVersion
            where TKeyOwner : notnull, IEquatable<TKeyOwner>
            where TKeyDependent : notnull, IEquatable<TKeyDependent>
    {
        TKeyOwner OwnerId { get; set; }
    }
}
