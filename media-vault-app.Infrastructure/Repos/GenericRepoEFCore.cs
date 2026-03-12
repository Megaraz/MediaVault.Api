using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Interfaces;

namespace media_vault_app.Infrastructure.Repos
{
    public class GenericRepoEFCore<TEntity, TKey> : IGenericRepoEFCore<TEntity, TKey> where TEntity : class, IEntityId<TKey>
    {
    }
}
