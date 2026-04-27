using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services
{
    public interface ISearchService<TEntity, TMinimalDto, TKey> 
        where TEntity : class, IEntity<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        Task<Result<IEnumerable<TMinimalDto>>> SearchAsync(string searchTerm, CancellationToken ct = default);
    }
}
