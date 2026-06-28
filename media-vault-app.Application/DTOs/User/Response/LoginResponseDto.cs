namespace media_vault_app.Application.DTOs.User.Response
{
    public record LoginResponseDto(UserDetailedDto User, string Token);
}
