using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Application.DTOs.User.Response
{
    public record UserDetailedDto(Guid Id, string Username, string Email, DateTime CreatedAtUtc) : IDtoIdentifiable<Guid>;

}
