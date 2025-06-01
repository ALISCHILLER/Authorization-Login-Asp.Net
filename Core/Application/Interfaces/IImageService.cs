using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس مدیریت تصاویر
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// آپلود تصویر
        /// </summary>
        /// <param name="imageData">داده‌های تصویر</param>
        /// <returns>آدرس تصویر آپلود شده</returns>
        Task<string> UploadImageAsync(byte[] imageData);

        /// <summary>
        /// حذف تصویر
        /// </summary>
        /// <param name="imageUrl">آدرس تصویر</param>
        /// <returns>نتیجه حذف</returns>
        Task<bool> DeleteImageAsync(string imageUrl);

        /// <summary>
        /// بررسی اعتبار تصویر
        /// </summary>
        /// <param name="imageData">داده‌های تصویر</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> ValidateImageAsync(byte[] imageData);

        /// <summary>
        /// تغییر اندازه تصویر
        /// </summary>
        /// <param name="imageData">داده‌های تصویر</param>
        /// <param name="width">عرض جدید</param>
        /// <param name="height">ارتفاع جدید</param>
        /// <returns>داده‌های تصویر تغییر اندازه داده شده</returns>
        Task<byte[]> ResizeImageAsync(byte[] imageData, int width, int height);
    }
}