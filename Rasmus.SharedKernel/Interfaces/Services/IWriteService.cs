using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services
{
    public interface IWriteService<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto> :
        ICreateService<TCreateDto, TDetailedDto>,
        IUpdateService<TKey, TUpdateDto>,
        IDeleteService<TKey>
        where TEntity : class, IEntityId<TKey>, new()
        where TDetailedDto : IDtoID<TKey>

    {
    }
}
