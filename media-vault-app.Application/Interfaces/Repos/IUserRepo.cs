using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Domain.Entities;
using Rasmus.SharedKernel.Interfaces;

namespace media_vault_app.Application.Interfaces.Repos
{
    // TODO: If there are any user-specific data access methods needed in the future, they can be added here.
    public interface IUserRepo : IGenericRepo<User, Guid>
    {
    }
}
