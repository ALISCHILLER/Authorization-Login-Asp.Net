using System;
using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    public class UserProfile : ValueObject
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; private set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; private set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; private set; }

        [Required]
        [MaxLength(500)]
        public string ProfileImageUrl { get; private set; }

        protected UserProfile() { }

        public static UserProfile Create(string firstName, string lastName, string profileImageUrl = "/images/default-profile.png")
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("نام نمی‌تواند خالی باشد", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("نام خانوادگی نمی‌تواند خالی باشد", nameof(lastName));

            return new UserProfile
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                FullName = $"{firstName.Trim()} {lastName.Trim()}",
                ProfileImageUrl = profileImageUrl?.Trim() ?? "/images/default-profile.png"
            };
        }

        public void Update(string firstName, string lastName, string profileImageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("نام نمی‌تواند خالی باشد", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("نام خانوادگی نمی‌تواند خالی باشد", nameof(lastName));

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            FullName = $"{firstName.Trim()} {lastName.Trim()}";
            if (profileImageUrl != null)
                ProfileImageUrl = profileImageUrl.Trim();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return FirstName;
            yield return LastName;
            yield return FullName;
            yield return ProfileImageUrl;
        }
    }
} 