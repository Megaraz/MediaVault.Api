namespace media_vault_app.Application.DTOs.User.Request
{
    public record UserLoginDto(string UsernameOrEmail, string Password);
}
