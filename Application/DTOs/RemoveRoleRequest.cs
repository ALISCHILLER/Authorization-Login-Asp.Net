using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    public class RemoveRoleRequest
    {
        [Required(ErrorMessage = "نام نقش الزامی است")]
        public string RoleName { get; set; }
    }
} 