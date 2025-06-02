using System;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    /// <summary>
    /// کلاس مقدار سؤال امنیتی
    /// </summary>
    public class SecurityQuestion
    {
        /// <summary>
        /// سؤال امنیتی
        /// </summary>
        public string Question { get; private set; }

        /// <summary>
        /// پاسخ هش شده
        /// </summary>
        public string HashedAnswer { get; private set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// تاریخ آخرین استفاده
        /// </summary>
        public DateTime? LastUsedAt { get; private set; }

        /// <summary>
        /// تعداد تلاش‌های ناموفق
        /// </summary>
        public int FailedAttempts { get; private set; }

        /// <summary>
        /// سازنده
        /// </summary>
        public SecurityQuestion(string question, string answer)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new ArgumentException("سؤال امنیتی نمی‌تواند خالی باشد", nameof(question));
            if (string.IsNullOrWhiteSpace(answer))
                throw new ArgumentException("پاسخ امنیتی نمی‌تواند خالی باشد", nameof(answer));

            Question = question.Trim();
            HashedAnswer = BCrypt.Net.BCrypt.HashPassword(answer.Trim());
            CreatedAt = DateTime.UtcNow;
            FailedAttempts = 0;
        }

        /// <summary>
        /// بررسی پاسخ
        /// </summary>
        public bool VerifyAnswer(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer))
                throw new ArgumentException("پاسخ امنیتی نمی‌تواند خالی باشد", nameof(answer));

            var isValid = BCrypt.Net.BCrypt.Verify(answer.Trim(), HashedAnswer);
            if (isValid)
            {
                LastUsedAt = DateTime.UtcNow;
                FailedAttempts = 0;
            }
            else
            {
                FailedAttempts++;
            }

            return isValid;
        }

        /// <summary>
        /// تغییر پاسخ
        /// </summary>
        public void ChangeAnswer(string newAnswer)
        {
            if (string.IsNullOrWhiteSpace(newAnswer))
                throw new ArgumentException("پاسخ امنیتی جدید نمی‌تواند خالی باشد", nameof(newAnswer));

            HashedAnswer = BCrypt.Net.BCrypt.HashPassword(newAnswer.Trim());
            LastUsedAt = DateTime.UtcNow;
            FailedAttempts = 0;
        }

        /// <summary>
        /// تغییر سؤال
        /// </summary>
        public void ChangeQuestion(string newQuestion)
        {
            if (string.IsNullOrWhiteSpace(newQuestion))
                throw new ArgumentException("سؤال امنیتی جدید نمی‌تواند خالی باشد", nameof(newQuestion));

            Question = newQuestion.Trim();
            LastUsedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// بازنشانی تعداد تلاش‌های ناموفق
        /// </summary>
        public void ResetFailedAttempts()
        {
            FailedAttempts = 0;
            LastUsedAt = DateTime.UtcNow;
        }

        public override bool Equals(object obj)
        {
            if (obj is SecurityQuestion other)
            {
                return Question.Equals(other.Question, StringComparison.OrdinalIgnoreCase) &&
                       HashedAnswer.Equals(other.HashedAnswer);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Question.ToLowerInvariant(), HashedAnswer);
        }

        public override string ToString()
        {
            return Question;
        }
    }
} 