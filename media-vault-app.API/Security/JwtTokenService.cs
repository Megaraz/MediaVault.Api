using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace media_vault_app.API.Security
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";
        public const int MinimumSecretKeyBytes = 32;
        public const int DefaultExpiryMinutes = 7 * 24 * 60;
        public const int MaximumExpiryMinutes = DefaultExpiryMinutes;

        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = DefaultExpiryMinutes;
    }

    public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
    {
        public ValidateOptionsResult Validate(string? name, JwtOptions options)
        {
            var failures = new List<string>();

            if (string.IsNullOrWhiteSpace(options.SecretKey))
            {
                failures.Add("JWT secret key must be configured.");
            }
            else if (Encoding.UTF8.GetByteCount(options.SecretKey) < JwtOptions.MinimumSecretKeyBytes)
            {
                failures.Add(
                    $"JWT secret key must be at least {JwtOptions.MinimumSecretKeyBytes} bytes.");
            }

            if (string.IsNullOrWhiteSpace(options.Issuer))
                failures.Add("JWT issuer must be configured.");

            if (string.IsNullOrWhiteSpace(options.Audience))
                failures.Add("JWT audience must be configured.");

            if (options.ExpiryMinutes <= 0 ||
                options.ExpiryMinutes > JwtOptions.MaximumExpiryMinutes)
            {
                failures.Add(
                    $"JWT expiry minutes must be between 1 and {JwtOptions.MaximumExpiryMinutes}.");
            }

            return failures.Count == 0
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(failures);
        }
    }

    public interface IJwtTokenService
    {
        string GenerateToken(Guid id, string username, string email);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly JwtOptions _options;
        private readonly SigningCredentials _credentials;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
            _key = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_options.SecretKey));
            _credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        }

        public string GenerateToken(Guid id, string username, string email)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
                signingCredentials: _credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
