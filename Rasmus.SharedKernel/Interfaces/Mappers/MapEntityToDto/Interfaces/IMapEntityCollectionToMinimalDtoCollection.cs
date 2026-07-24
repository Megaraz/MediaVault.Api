namespace Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces
{
    public interface IMapEntityCollectionToMinimalDtoCollection<TEntity, TMinimalDto>
    {
        IReadOnlyList<TMinimalDto> ToMinimalDtoCollection(IEnumerable<TEntity> entities);
    }
}
