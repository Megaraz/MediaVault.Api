namespace Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces
{
    public interface IMapUpdateDtoToEntity<TEntity, TKey, TUpdateDto>
    {
        TEntity ToEntity(TKey id, TUpdateDto updateDto);
    }
}
