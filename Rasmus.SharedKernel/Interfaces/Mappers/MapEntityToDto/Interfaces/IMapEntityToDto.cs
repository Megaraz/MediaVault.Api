using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces
{
    public interface IMapEntityToDto<TEntity, TKey, TDetailedDto, TMinimalDto> :
        IMapEntityToDetailedDto<TEntity, TDetailedDto>,
        IMapEntityToMinimalDto<TEntity, TMinimalDto>,
        IMapEntityCollectionToDetailedDtoCollection<TEntity, TDetailedDto>,
        IMapEntityCollectionToMinimalDtoCollection<TEntity, TMinimalDto>
        where TEntity : class, IEntityId<TKey>, new()
        where TDetailedDto : IDtoID<TKey>
        where TMinimalDto : IDtoID<TKey>
    {
    }
}
