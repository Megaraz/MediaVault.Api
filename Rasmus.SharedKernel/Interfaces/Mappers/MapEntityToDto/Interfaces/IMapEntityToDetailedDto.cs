using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces
{
    public interface IMapEntityToDetailedDto<TEntity, TDetailedDto>
    {
        TDetailedDto ToDetailedDto(TEntity entity);
    }
}
