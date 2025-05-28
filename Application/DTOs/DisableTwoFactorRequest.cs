using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    public class DisableTwoFactorRequest
    {
        [Required(ErrorMessage = "کد تأیید الزامی است")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "کد تأیید باید 6 رقمی باشد")]
        public string Code { get; set; }
    }
} 