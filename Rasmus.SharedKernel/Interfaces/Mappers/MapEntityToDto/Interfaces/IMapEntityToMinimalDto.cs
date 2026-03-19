using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces
{
    public interface IMapEntityToMinimalDto<TEntity, TMinimalDto>
    {
        TMinimalDto ToMinimalDTO(TEntity entity);
    }
}
