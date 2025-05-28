using System;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// پاسخ درخواست راه‌اندازی احراز هویت دو مرحله‌ای
    /// </summary>
    public class TwoFactorSetupResponse
    {
        /// <summary>
        /// نوع احراز هویت دو مرحله‌ای
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// کلید مخفی برای اپلیکیشن احراز هویت
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// آدرس QR کد برای اسکن
        /// </summary>
        public string QrCodeUrl { get; set; }

        /// <summary>
        /// لیست کدهای بازیابی
        /// </summary>
        public List<string> RecoveryCodes { get; set; }

        /// <summary>
        /// تاریخ انقضای کدهای بازیابی
        /// </summary>
        public DateTime RecoveryCodesExpiresAt { get; set; }

        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public string[] RecoveryCodesArray { get; set; }
    }
} 