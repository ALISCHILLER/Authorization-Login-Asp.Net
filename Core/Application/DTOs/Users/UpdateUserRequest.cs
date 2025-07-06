using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Users
{
    public class UpdateUserRequest
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        public string UserName { get => Username; set => Username = value; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        public bool IsActive { get; set; }

        public Guid Id { get; set; }
    }
}