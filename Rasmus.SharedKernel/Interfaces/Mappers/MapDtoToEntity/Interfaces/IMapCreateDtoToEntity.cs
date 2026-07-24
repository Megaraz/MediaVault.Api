namespace Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces
{
    public interface IMapCreateDtoToEntity<TEntity, TCreateDto>
    {
        TEntity ToEntity(TCreateDto createDto);
    }
}
