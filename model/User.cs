using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.model
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public UserRole Role { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;
    }
    public enum UserRole
    {
        Admin,
        Operator,
        Guest
    }

}
