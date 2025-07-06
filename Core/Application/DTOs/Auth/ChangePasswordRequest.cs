using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    /// <summary>
    /// درخواست تغییر رمز عبور
    /// </summary>
    public class ChangePasswordRequest : AuthRequest
    {
        /// <summary>
        /// رمز عبور فعلی
        /// </summary>
        [Required(ErrorMessage = "رمز عبور فعلی الزامی است")]
        public string CurrentPassword { get; set; }

        /// <summary>
        /// رمز عبور جدید
        /// </summary>
        [Required(ErrorMessage = "رمز عبور جدید الزامی است")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "رمز عبور باید بین 8 تا 100 کاراکتر باشد")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "رمز عبور باید شامل حداقل یک حرف کوچک، یک حرف بزرگ، یک عدد و یک کاراکتر خاص باشد")]
        public string NewPassword { get; set; }

        /// <summary>
        /// تأیید رمز عبور جدید
        /// </summary>
        [Required(ErrorMessage = "تأیید رمز عبور جدید الزامی است")]
        [Compare("NewPassword", ErrorMessage = "رمز عبور جدید و تأیید آن مطابقت ندارند")]
        public string ConfirmNewPassword { get; set; }

        /// <summary>
        /// اطلاعات دستگاه
        /// </summary>
        [Required(ErrorMessage = "اطلاعات دستگاه الزامی است")]
        public DeviceInfoDto DeviceInfo { get; set; }

        /// <summary>
        /// موقعیت مکانی
        /// </summary>
        public LocationDto Location { get; set; }

        /// <summary>
        /// آیا تغییر رمز عبور اجباری است؟
        /// </summary>
        public bool RequirePasswordChange { get; set; }
    }
} 