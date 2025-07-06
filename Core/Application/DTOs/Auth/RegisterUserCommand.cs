namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    public class RegisterUserCommand
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
