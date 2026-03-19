using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces
{
    public interface IMapDtoToEntity<TEntity, TDetailedDto, TCreateDto, TKey, TUpdateDto> : 
        IMapDtoCollectionToEntityCollection<TEntity, TDetailedDto>,
        IMapCreateDtoToEntity<TEntity, TCreateDto>,
        IMapDetailedDtoToEntity<TEntity, TDetailedDto>,
        IMapUpdateDtoToEntity<TEntity, TKey, TUpdateDto>

    {
    }
}
