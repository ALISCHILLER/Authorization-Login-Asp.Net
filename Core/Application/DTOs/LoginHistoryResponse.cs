using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs
{
    /// <summary>
    /// پاسخ صفحه‌بندی شده تاریخچه ورود
    /// </summary>
    public class LoginHistoryResponse
    {
        /// <summary>
        /// شماره صفحه
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// تعداد در هر صفحه
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// تعداد کل آیتم‌های تاریخچه ورود
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// تعداد کل صفحات
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// آیا صفحه بعدی وجود دارد
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// آیا صفحه قبلی وجود دارد
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// لیست آیتم‌های تاریخچه ورود
        /// </summary>
        public IEnumerable<LoginHistoryItem> Items { get; set; }
    }
}