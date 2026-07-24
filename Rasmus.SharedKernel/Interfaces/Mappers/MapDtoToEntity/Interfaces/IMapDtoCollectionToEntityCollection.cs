namespace Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces
{
    public interface IMapDtoCollectionToEntityCollection<TEntity, TDetailedDto>
    {
        IEnumerable<TEntity> ToEntities(IEnumerable<TDetailedDto> detailedDtos);
    }
}
