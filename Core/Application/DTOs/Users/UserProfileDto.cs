namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Users
{
    public class UserProfileDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfileImageUrl { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
