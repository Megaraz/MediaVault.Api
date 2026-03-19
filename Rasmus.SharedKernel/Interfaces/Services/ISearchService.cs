using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services
{
    public interface ISearchService<TEntity, TMinimalDto, TKey> 
        where TEntity : class, IEntityId<TKey>, new()
    {
        Task<Result<IEnumerable<TMinimalDto>>> SearchAsync(string searchTerm, CancellationToken ct = default);
    }
}
