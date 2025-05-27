namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس مدیریت رمز عبور
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// هش کردن رمز عبور
        /// </summary>
        /// <param name="password">رمز عبور</param>
        /// <returns>هش رمز عبور</returns>
        string HashPassword(string password);

        /// <summary>
        /// بررسی تطابق رمز عبور با هش
        /// </summary>
        /// <param name="password">رمز عبور</param>
        /// <param name="hashedPassword">هش رمز عبور</param>
        /// <returns>نتیجه بررسی</returns>
        bool VerifyPassword(string password, string hashedPassword);
    }
} 