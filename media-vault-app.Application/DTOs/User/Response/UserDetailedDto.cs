using Rasmus.SharedKernel.Interfaces.Identifiers;

namespace media_vault_app.Application.DTOs.User.Response
{
    public record UserDetailedDto(
        Guid Id,
        string Username,
        string Email,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc,
        int Version) : IDtoIdentifiable<Guid>;

}
