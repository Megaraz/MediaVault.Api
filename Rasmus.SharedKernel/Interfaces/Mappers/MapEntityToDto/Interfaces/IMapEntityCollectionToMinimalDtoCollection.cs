using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces
{
    public interface IMapEntityCollectionToMinimalDtoCollection<TEntity, TMinimalDto>
    {
        IEnumerable<TMinimalDto> ToMinimalDtoCollection(IEnumerable<TEntity> entities);
    }
}
