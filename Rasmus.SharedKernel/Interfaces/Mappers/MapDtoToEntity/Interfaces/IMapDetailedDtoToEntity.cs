namespace Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces
{
    public interface IMapDetailedDtoToEntity<TEntity, TDetailedDto>
    {
        TEntity ToEntity(TDetailedDto detailedDto);
    }
}
