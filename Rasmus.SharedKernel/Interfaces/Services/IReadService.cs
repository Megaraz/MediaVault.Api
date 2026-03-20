using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace Rasmus.SharedKernel.Interfaces.Services
{
    public interface IReadService<TEntity, TKey, TDetailedDto, TMinimalDto> :
        IGetCollectionService<TDetailedDto, TMinimalDto>,
        IGetByIdService<TKey, TDetailedDto>
        where TEntity : class, IEntityId<TKey>
        where TDetailedDto : IDtoID<TKey>
        where TMinimalDto : IDtoID<TKey>

    {
    }
}
