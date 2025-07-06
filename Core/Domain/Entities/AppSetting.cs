using System;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// موجودیت تنظیمات برنامه
    /// </summary>
    public class AppSetting : BaseEntity
    {
        /// <summary>
        /// کلید تنظیم
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// مقدار تنظیم
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// نوع داده
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// گروه تنظیم
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// اولویت نمایش
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// آیا قابل ویرایش است؟
        /// </summary>
        public bool IsEditable { get; set; }

        /// <summary>
        /// آیا حساس است؟
        /// </summary>
        public bool IsSensitive { get; set; }

        /// <summary>
        /// آخرین زمان به‌روزرسانی
        /// </summary>
        public DateTime? LastModifiedAt { get; set; }

        /// <summary>
        /// آخرین کاربر ویرایش کننده
        /// </summary>
        public Guid? LastModifiedBy { get; set; }

        protected AppSetting() { }

        public AppSetting(
            string key,
            string value,
            string description = null,
            string dataType = "string",
            string group = "General",
            int displayOrder = 0,
            bool isEditable = true,
            bool isSensitive = false)
        {
            Id = Guid.NewGuid();
            Key = key;
            Value = value;
            Description = description;
            DataType = dataType;
            Group = group;
            DisplayOrder = displayOrder;
            IsEditable = isEditable;
            IsSensitive = isSensitive;
        }

        /// <summary>
        /// به‌روزرسانی مقدار تنظیم
        /// </summary>
        public void UpdateValue(string value, Guid userId)
        {
            if (!IsEditable)
                throw new InvalidOperationException("این تنظیم قابل ویرایش نیست");

            Value = value;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = userId;
        }
    }
}