using System;
using System.Collections.Generic;
using System.Text;

namespace media_vault_app.Application.DTOs.User.Request
{
    public record UserRegisterDto(string Username, string Email, string ConfirmEmail, string Password, string ConfirmPassword);
}
