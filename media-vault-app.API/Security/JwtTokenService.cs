using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using media_vault_app.Application.DTOs.User.Response;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace media_vault_app.API.Security
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryDays { get; set; } = 7;
    }

    public interface IJwtTokenService
    {
        string GenerateToken(UserDetailedDto user);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _options;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public string GenerateToken(UserDetailedDto user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_options.ExpiryDays),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
