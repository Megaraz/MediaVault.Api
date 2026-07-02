using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace media_vault_app.API.Controllers
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool TryGetUserId(this ClaimsPrincipal user, out Guid userId)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) 
                ?? user.FindFirst(JwtRegisteredClaimNames.Sub);

            if (Guid.TryParse(userIdClaim?.Value, out userId))
                return true;

            userId = Guid.Empty;
            return false;
        }
    }
}
