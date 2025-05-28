using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    public class TwoFactorDto
    {
        [Required(ErrorMessage = "کد تأیید الزامی است")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "کد تأیید باید 6 رقمی باشد")]
        public string Code { get; set; }

        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public string UserId { get; set; }

        public bool RememberMe { get; set; }
    }
} 