namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Users
{
    public class ChangePasswordRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
