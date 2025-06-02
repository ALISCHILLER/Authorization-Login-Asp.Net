using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Common
{
    /// <summary>
    /// کلاس پایه برای درخواست‌های صفحه‌بندی شده
    /// </summary>
    public class PaginationRequestDto
    {
        /// <summary>
        /// شماره صفحه (شروع از 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "شماره صفحه باید بزرگتر از صفر باشد")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// تعداد آیتم در هر صفحه
        /// </summary>
        [Range(1, 100, ErrorMessage = "تعداد آیتم در هر صفحه باید بین 1 تا 100 باشد")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// فیلد مرتب‌سازی
        /// </summary>
        public string SortBy { get; set; }

        /// <summary>
        /// جهت مرتب‌سازی (صعودی یا نزولی)
        /// </summary>
        public bool SortAscending { get; set; } = true;
    }

    /// <summary>
    /// کلاس پایه برای پاسخ‌های صفحه‌بندی شده
    /// </summary>
    public class PaginationResponseDto<T>
    {
        /// <summary>
        /// لیست آیتم‌ها
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// تعداد کل آیتم‌ها
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// تعداد کل صفحات
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// شماره صفحه فعلی
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// تعداد آیتم در هر صفحه
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// آیا صفحه بعدی وجود دارد؟
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// آیا صفحه قبلی وجود دارد؟
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }
} 