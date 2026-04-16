using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces
{
    public interface IMapDtoToEntity<TEntity, TDetailedDto, TCreateDto, TUpdateDto, TKey> :
        IMapDtoCollectionToEntityCollection<TEntity, TDetailedDto>,
        IMapCreateDtoToEntity<TEntity, TCreateDto>,
        IMapUpdateDtoToEntity<TEntity, TKey, TUpdateDto>,
        IMapDetailedDtoToEntity<TEntity, TDetailedDto>

    {
    }
}
