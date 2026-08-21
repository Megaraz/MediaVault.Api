using media_vault_app.Application.DTOs.User.Request;

namespace media_vault_app.Application.Identity;

public static class UserIdentifierCanonicalizer
{
    public const string UsernameCasePolicy = "lowercase";

    public static string CanonicalizeUsername(string? value) =>
        Canonicalize(value);

    public static string CanonicalizeEmail(string? value) =>
        Canonicalize(value);

    public static string CanonicalizeLoginIdentifier(string? value) =>
        Canonicalize(value);

    public static UserLoginDto Canonicalize(UserLoginDto dto) =>
        dto with
        {
            UsernameOrEmail = CanonicalizeLoginIdentifier(dto.UsernameOrEmail)
        };

    public static UserRegisterDto Canonicalize(UserRegisterDto dto) =>
        dto with
        {
            Username = CanonicalizeUsername(dto.Username),
            Email = CanonicalizeEmail(dto.Email),
            ConfirmEmail = CanonicalizeEmail(dto.ConfirmEmail)
        };

    public static UserUpdateDto Canonicalize(UserUpdateDto dto) =>
        new()
        {
            UserName = CanonicalizeUsername(dto.UserName),
            Email = CanonicalizeEmail(dto.Email),
            ExpectedVersion = dto.ExpectedVersion
        };

    private static string Canonicalize(string? value) =>
        value?.Trim().ToLowerInvariant() ?? string.Empty;
}
