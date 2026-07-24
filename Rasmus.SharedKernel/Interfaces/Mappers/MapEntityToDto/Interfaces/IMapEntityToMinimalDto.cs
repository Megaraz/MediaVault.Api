namespace Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces
{
    public interface IMapEntityToMinimalDto<TEntity, TMinimalDto>
    {
        TMinimalDto ToMinimalDto(TEntity entity);
    }
}
