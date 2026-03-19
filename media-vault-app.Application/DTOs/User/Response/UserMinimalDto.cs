using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces;

namespace media_vault_app.Application.DTOs.User.Response
{
    public record UserMinimalDto(Guid Id, string Username, string Email) : IDtoID<Guid>;
}
