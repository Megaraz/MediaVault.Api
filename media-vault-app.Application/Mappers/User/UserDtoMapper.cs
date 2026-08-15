using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.DTOs.User.Response;
using media_vault_app.Application.Identity;
using media_vault_app.Application.Interfaces.Mappers;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Application.Mappers.User
{
    public class UserDtoMapper : IUserDtoMapper
    {
        public UserEntity ToEntity(UserRegisterDto createDto) =>
            new()
            {
                Id = Guid.NewGuid(),
                Username = UserIdentifierCanonicalizer.CanonicalizeUsername(createDto.Username),
                Email = UserIdentifierCanonicalizer.CanonicalizeEmail(createDto.Email),
                PasswordHash = createDto.Password,
                CreatedAtUtc = DateTime.UtcNow
            };

        public UserEntity ToEntity(UserDetailedDto detailedDto) =>
            new()
            {
                Id = detailedDto.Id,
                Username = UserIdentifierCanonicalizer.CanonicalizeUsername(detailedDto.Username),
                Email = UserIdentifierCanonicalizer.CanonicalizeEmail(detailedDto.Email),
                CreatedAtUtc = detailedDto.CreatedAtUtc
            };

        public IEnumerable<UserEntity> ToEntities(IEnumerable<UserDetailedDto> detailedDtos) =>
            detailedDtos.Select(ToEntity);

        public UserEntity ToEntity(Guid id, UserUpdateDto updateDto) =>
            new()
            {
                Id = id,
                Username = UserIdentifierCanonicalizer.CanonicalizeUsername(updateDto.UserName),
                Email = UserIdentifierCanonicalizer.CanonicalizeEmail(updateDto.Email)
            };
    }
}
