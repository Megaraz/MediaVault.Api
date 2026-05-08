using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Services.CrudServiceInterfaces;

namespace Rasmus.SharedKernel.Interfaces.Services
{
    public interface IReadService<TEntity, TKey, TDetailedDto, TMinimalDto> :
        IGetCollectionService<TDetailedDto, TMinimalDto>,
        IGetByIdService<TKey, TDetailedDto>
        where TEntity : class, IEntity<TKey>
        where TDetailedDto : IDtoIdentifiable<TKey>
        where TMinimalDto : IDtoIdentifiable<TKey>
        where TKey : notnull, IEquatable<TKey>

    {
    }
}
