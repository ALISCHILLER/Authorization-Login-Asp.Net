using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس هش کردن رمز عبور
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// هش کردن رمز عبور
        /// </summary>
        /// <param name="password">رمز عبور</param>
        /// <returns>هش و نمک</returns>
        Task<(string hash, string salt)> HashPasswordAsync(string password);

        /// <summary>
        /// بررسی تطابق رمز عبور با هش
        /// </summary>
        /// <param name="password">رمز عبور</param>
        /// <param name="hash">هش</param>
        /// <param name="salt">نمک</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> VerifyPasswordAsync(string password, string hash, string salt);

        /// <summary>
        /// بررسی نیاز به بروزرسانی هش
        /// </summary>
        /// <param name="hash">هش</param>
        /// <returns>نتیجه بررسی</returns>
        bool NeedsRehash(string hash);
    }
}