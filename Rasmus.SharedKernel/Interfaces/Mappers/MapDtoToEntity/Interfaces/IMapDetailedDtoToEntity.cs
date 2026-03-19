using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces
{
    public interface IMapDetailedDtoToEntity<TEntity, TDetailedDto>
    {
        TEntity ToEntity(TDetailedDto detailedDto);
    }
}
