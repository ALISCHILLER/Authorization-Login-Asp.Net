using System;
using System.Collections.Generic;
using System.Linq;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    /// <summary>
    /// کلاس مقدار مجموعه سؤالات امنیتی
    /// </summary>
    public class SecurityQuestionSet
    {
        private readonly List<SecurityQuestion> _questions;
        private const int MinRequiredQuestions = 3;
        private const int MaxQuestions = 5;

        /// <summary>
        /// سؤالات امنیتی
        /// </summary>
        public IReadOnlyList<SecurityQuestion> Questions => _questions.AsReadOnly();

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// تاریخ آخرین تغییر
        /// </summary>
        public DateTime LastModifiedAt { get; private set; }

        /// <summary>
        /// سازنده
        /// </summary>
        public SecurityQuestionSet(IEnumerable<(string question, string answer)> questions)
        {
            if (questions == null)
                throw new ArgumentNullException(nameof(questions));

            var questionList = questions.ToList();
            if (questionList.Count < MinRequiredQuestions)
                throw new ArgumentException($"حداقل {MinRequiredQuestions} سؤال امنیتی مورد نیاز است", nameof(questions));
            if (questionList.Count > MaxQuestions)
                throw new ArgumentException($"حداکثر {MaxQuestions} سؤال امنیتی مجاز است", nameof(questions));

            _questions = questionList.Select(q => new SecurityQuestion(q.question, q.answer)).ToList();
            CreatedAt = DateTime.UtcNow;
            LastModifiedAt = CreatedAt;
        }

        /// <summary>
        /// بررسی پاسخ‌ها
        /// </summary>
        public bool VerifyAnswers(IEnumerable<(string question, string answer)> answers)
        {
            if (answers == null)
                throw new ArgumentNullException(nameof(answers));

            var answerList = answers.ToList();
            if (answerList.Count != _questions.Count)
                return false;

            var questionDict = _questions.ToDictionary(q => q.Question, q => q, StringComparer.OrdinalIgnoreCase);
            return answerList.All(a => 
                questionDict.TryGetValue(a.question, out var question) && 
                question.VerifyAnswer(a.answer));
        }

        /// <summary>
        /// تغییر پاسخ یک سؤال
        /// </summary>
        public void ChangeAnswer(string question, string newAnswer)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new ArgumentException("سؤال امنیتی نمی‌تواند خالی باشد", nameof(question));
            if (string.IsNullOrWhiteSpace(newAnswer))
                throw new ArgumentException("پاسخ امنیتی جدید نمی‌تواند خالی باشد", nameof(newAnswer));

            var securityQuestion = _questions.FirstOrDefault(q => 
                q.Question.Equals(question, StringComparison.OrdinalIgnoreCase));

            if (securityQuestion == null)
                throw new ArgumentException("سؤال امنیتی یافت نشد", nameof(question));

            securityQuestion.ChangeAnswer(newAnswer);
            LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// تغییر یک سؤال
        /// </summary>
        public void ChangeQuestion(string oldQuestion, string newQuestion, string answer)
        {
            if (string.IsNullOrWhiteSpace(oldQuestion))
                throw new ArgumentException("سؤال امنیتی قدیمی نمی‌تواند خالی باشد", nameof(oldQuestion));
            if (string.IsNullOrWhiteSpace(newQuestion))
                throw new ArgumentException("سؤال امنیتی جدید نمی‌تواند خالی باشد", nameof(newQuestion));
            if (string.IsNullOrWhiteSpace(answer))
                throw new ArgumentException("پاسخ امنیتی نمی‌تواند خالی باشد", nameof(answer));

            var index = _questions.FindIndex(q => 
                q.Question.Equals(oldQuestion, StringComparison.OrdinalIgnoreCase));

            if (index == -1)
                throw new ArgumentException("سؤال امنیتی یافت نشد", nameof(oldQuestion));

            _questions[index] = new SecurityQuestion(newQuestion, answer);
            LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// بازنشانی تعداد تلاش‌های ناموفق برای همه سؤالات
        /// </summary>
        public void ResetAllFailedAttempts()
        {
            foreach (var question in _questions)
            {
                question.ResetFailedAttempts();
            }
            LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// دریافت تعداد کل تلاش‌های ناموفق
        /// </summary>
        public int GetTotalFailedAttempts()
        {
            return _questions.Sum(q => q.FailedAttempts);
        }

        public override bool Equals(object obj)
        {
            if (obj is SecurityQuestionSet other)
            {
                return _questions.Count == other._questions.Count &&
                       _questions.All(q => other._questions.Contains(q));
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _questions.Aggregate(0, (hash, question) => 
                hash ^ question.GetHashCode());
        }

        public override string ToString()
        {
            return string.Join(", ", _questions.Select(q => q.Question));
        }
    }
} 