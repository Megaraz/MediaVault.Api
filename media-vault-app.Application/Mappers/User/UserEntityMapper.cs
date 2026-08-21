using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Identity;
using media_vault_app.Application.Interfaces.Mappers;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Mappers.User
{
    public class UserEntityMapper : IUserEntityMapper
    {
        public UserDetailedDto ToDetailedDto(UserEntity entity) =>
            new(
                entity.Id,
                UserIdentifierCanonicalizer.CanonicalizeUsername(entity.Username),
                UserIdentifierCanonicalizer.CanonicalizeEmail(entity.Email),
                entity.CreatedAtUtc,
                entity.UpdatedAtUtc);

        public IReadOnlyList<UserDetailedDto> ToDetailedDtoCollection(IEnumerable<UserEntity> entities) =>
            entities.Select(ToDetailedDto).ToList();

        public UserMinimalDto ToMinimalDto(UserEntity entity) =>
            new(
                entity.Id,
                UserIdentifierCanonicalizer.CanonicalizeUsername(entity.Username),
                UserIdentifierCanonicalizer.CanonicalizeEmail(entity.Email));

        public IReadOnlyList<UserMinimalDto> ToMinimalDtoCollection(IEnumerable<UserEntity> entities) =>
            entities.Select(ToMinimalDto).ToList();

    }
}
