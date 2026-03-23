using System;
using System.Collections.Generic;
using System.Text;

namespace Rasmus.SharedKernel.Interfaces.Mappers.MapDtoToEntity.Interfaces
{
    public interface IMapUpdateDtoToEntity<TEntity, TKey, TUpdateDto>
    {
        TEntity MapToEntity(TKey id, TUpdateDto updateDto);
    }
}
