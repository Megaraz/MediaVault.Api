using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services
{
    public interface IServiceBase<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto, TMinimalDto> :
        IWriteService<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto>,
        IReadService<TEntity, TKey, TDetailedDto, TMinimalDto>
        where TEntity : class, IEntityId<TKey>, new()
        where TDetailedDto : IDtoID<TKey>
        where TMinimalDto : IDtoID<TKey>
    {


    }
}
