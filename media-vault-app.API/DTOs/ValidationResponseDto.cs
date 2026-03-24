namespace media_vault_app.API.DTOs
{
    public sealed record ValidationResponseDto(string Message, IEnumerable<string>? ValidationErrors);
}
