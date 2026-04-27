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
        where TEntity : IEntity<TKey>
        where TDetailedDto : IDtoIdentifiable<TKey>
        where TMinimalDto : IDtoIdentifiable<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
    }
}
