using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Infrastructure.Options;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    /// <summary>
    /// سرویس مدیریت تصاویر
    /// </summary>
    public class ImageService : IImageService
    {
        private readonly ILogger<ImageService> _logger;
        private readonly ImageServiceOptions _options;
        private readonly string _uploadPath;

        public ImageService(
            ILogger<ImageService> logger,
            IOptions<ImageServiceOptions> options)
        {
            _logger = logger;
            _options = options.Value;
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), _options.UploadPath);
            
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        /// <inheritdoc/>
        public async Task<string> UploadImageAsync(byte[] imageData)
        {
            try
            {
                if (!await ValidateImageAsync(imageData))
                {
                    throw new ArgumentException("تصویر نامعتبر است");
                }

                var fileName = $"{Guid.NewGuid()}.jpg";
                var filePath = Path.Combine(_uploadPath, fileName);

                // تغییر اندازه تصویر قبل از ذخیره
                var resizedImage = await ResizeImageAsync(imageData, _options.MaxWidth, _options.MaxHeight);
                await File.WriteAllBytesAsync(filePath, resizedImage);

                _logger.LogInformation($"تصویر با موفقیت آپلود شد: {fileName}");
                return $"/uploads/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در آپلود تصویر");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return false;
                }

                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_uploadPath, fileName);

                if (!File.Exists(filePath))
                {
                    return false;
                }

                await Task.Run(() => File.Delete(filePath));
                _logger.LogInformation($"تصویر با موفقیت حذف شد: {fileName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف تصویر");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateImageAsync(byte[] imageData)
        {
            try
            {
                if (imageData == null || imageData.Length == 0)
                {
                    return false;
                }

                if (imageData.Length > _options.MaxFileSize)
                {
                    return false;
                }

                using var image = await Image.LoadAsync(new MemoryStream(imageData));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی اعتبار تصویر");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]> ResizeImageAsync(byte[] imageData, int width, int height)
        {
            try
            {
                using var image = await Image.LoadAsync(new MemoryStream(imageData));
                
                // محاسبه نسبت تصویر
                var ratio = Math.Min(
                    (float)width / image.Width,
                    (float)height / image.Height
                );

                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                image.Mutate(x => x.Resize(newWidth, newHeight));

                using var outputStream = new MemoryStream();
                await image.SaveAsJpegAsync(outputStream);
                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تغییر اندازه تصویر");
                throw;
            }
        }
    }
} 