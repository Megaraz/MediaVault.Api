using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Services.CrudServiceInterfaces;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services
{
    public interface IWriteService<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto> :
        ICreateService<TCreateDto, TDetailedDto>,
        IUpdateService<TKey, TUpdateDto>,
        IDeleteService<TKey>
        where TEntity : class, IEntity<TKey>
        where TDetailedDto : IDtoIdentifiable<TKey>
        where TKey : notnull, IEquatable<TKey>

    {
    }
}
