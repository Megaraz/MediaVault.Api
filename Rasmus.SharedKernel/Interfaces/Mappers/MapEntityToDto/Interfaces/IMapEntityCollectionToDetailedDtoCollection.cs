using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces
{
    public interface IMapEntityCollectionToDetailedDtoCollection<TEntity, TDetailedDto>
    {
        IEnumerable<TDetailedDto> ToDetailedDtoCollection(IEnumerable<TEntity> entities);
    }
}
