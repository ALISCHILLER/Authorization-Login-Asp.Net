using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// اینترفیس مدیریت احراز هویت دو مرحله‌ای
    /// </summary>
    public interface ITwoFactorAuthenticator
    {
        /// <summary>
        /// تولید کلید مخفی برای احراز هویت دو مرحله‌ای
        /// </summary>
        /// <returns>کلید مخفی</returns>
        string GenerateSecretKey();

        /// <summary>
        /// تولید کد یکبار مصرف
        /// </summary>
        /// <param name="secretKey">کلید مخفی</param>
        /// <returns>کد یکبار مصرف</returns>
        string GenerateCode(string secretKey);

        /// <summary>
        /// بررسی اعتبار کد یکبار مصرف
        /// </summary>
        /// <param name="secretKey">کلید مخفی</param>
        /// <param name="code">کد یکبار مصرف</param>
        /// <returns>نتیجه بررسی</returns>
        bool ValidateCode(string secretKey, string code);

        /// <summary>
        /// تولید کدهای بازیابی
        /// </summary>
        /// <param name="count">تعداد کدها</param>
        /// <returns>کدهای بازیابی</returns>
        string[] GenerateRecoveryCodes(int count = 10);

        /// <summary>
        /// ارسال کد یکبار مصرف
        /// </summary>
        /// <param name="user">کاربر</param>
        /// <param name="code">کد یکبار مصرف</param>
        Task SendCodeAsync(User user, string code);
    }
}