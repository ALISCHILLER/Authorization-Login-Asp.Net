using System;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    /// <summary>
    /// درخواست تأیید دو مرحله‌ای
    /// </summary>
    public class TwoFactorRequest : AuthRequest
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public Guid UserId { get; set; }

        /// <summary>
        /// کد تأیید
        /// </summary>
        [Required(ErrorMessage = "کد تأیید الزامی است")]
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "کد تأیید باید 6 رقمی باشد")]
        public string Code { get; set; }

        /// <summary>
        /// روش ارسال کد تأیید
        /// </summary>
        [Required(ErrorMessage = "روش ارسال کد تأیید الزامی است")]
        public TwoFactorType Provider { get; set; }

        /// <summary>
        /// به خاطر سپاری دستگاه
        /// </summary>
        public bool RememberDevice { get; set; }

        /// <summary>
        /// اطلاعات دستگاه
        /// </summary>
        [Required(ErrorMessage = "اطلاعات دستگاه الزامی است")]
        public DeviceInfoDto DeviceInfo { get; set; }

        /// <summary>
        /// موقعیت مکانی
        /// </summary>
        public LocationDto Location { get; set; }
    }
} 