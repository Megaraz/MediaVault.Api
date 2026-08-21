using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Identity;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Mappers.User;

public static class UserAccountMapper
{
    public static UserDetailedDto ToDetailedDto(UserEntity entity) =>
        new(
            entity.Id,
            UserIdentifierCanonicalizer.CanonicalizeUsername(entity.Username),
            UserIdentifierCanonicalizer.CanonicalizeEmail(entity.Email),
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc,
            entity.Version);

    public static UserEntity ToRegistrationEntity(UserRegisterDto registerDto) =>
        new()
        {
            Id = Guid.NewGuid(),
            Username = UserIdentifierCanonicalizer.CanonicalizeUsername(registerDto.Username),
            Email = UserIdentifierCanonicalizer.CanonicalizeEmail(registerDto.Email),
            PasswordHash = registerDto.Password
        };
}
