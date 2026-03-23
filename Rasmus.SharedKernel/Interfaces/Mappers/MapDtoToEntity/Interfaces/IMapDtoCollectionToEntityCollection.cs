using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces
{
    public interface IMapDtoCollectionToEntityCollection<TEntity, TDetailedDto>
    {
        IEnumerable<TEntity> ToEntities(IEnumerable<TDetailedDto> detailedDtos);
    }
}
