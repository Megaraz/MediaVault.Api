using System;
using System.Collections.Generic;
using System.Text;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IPasswordHasherService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string providedPassword);
    }
}
