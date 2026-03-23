using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace media_vault_app.API.Security
{
    public class PasswordHasherService : IPasswordHasherService
    {
        private readonly PasswordHasher<User> _passwordHasher = new();

        public string HashPassword(string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            return _passwordHasher.HashPassword(new User(), password);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(providedPassword))
            {
                return false;
            }

            var result = _passwordHasher.VerifyHashedPassword(
                new User(),
                hashedPassword,
                providedPassword);

            return result is PasswordVerificationResult.Success
                or PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
