namespace media_vault_app.Application.DTOs.User.Request
{
    public class UserUpdateDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        public int ExpectedVersion { get; set; }

    }
}
