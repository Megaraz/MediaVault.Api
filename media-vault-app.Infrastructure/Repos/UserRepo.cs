using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Domain.Entities;

namespace media_vault_app.Infrastructure.Repos
{
    public class UserRepo : GenericRepoEFCore<User, Guid>, IUserRepo
    {
        public UserRepo(AppDbContext appDbContext) : base(appDbContext)
        {
        }
    }
}
