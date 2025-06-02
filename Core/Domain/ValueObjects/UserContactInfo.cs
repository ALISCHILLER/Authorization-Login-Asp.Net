using System;
using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    public class UserContactInfo : ValueObject
    {
        [Required]
        public Email Email { get; private set; }

        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; private set; }

        public bool IsEmailVerified { get; private set; }
        public bool IsPhoneVerified { get; private set; }

        protected UserContactInfo() { }

        public static UserContactInfo Create(string email, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("ایمیل نمی‌تواند خالی باشد", nameof(email));
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("شماره تلفن نمی‌تواند خالی باشد", nameof(phoneNumber));

            return new UserContactInfo
            {
                Email = new Email(email.Trim()),
                PhoneNumber = phoneNumber.Trim(),
                IsEmailVerified = false,
                IsPhoneVerified = false
            };
        }

        public void UpdateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("ایمیل نمی‌تواند خالی باشد", nameof(email));

            Email = new Email(email.Trim());
            IsEmailVerified = false;
        }

        public void UpdatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("شماره تلفن نمی‌تواند خالی باشد", nameof(phoneNumber));

            PhoneNumber = phoneNumber.Trim();
            IsPhoneVerified = false;
        }

        public void VerifyEmail()
        {
            IsEmailVerified = true;
        }

        public void VerifyPhone()
        {
            IsPhoneVerified = true;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Email;
            yield return PhoneNumber;
            yield return IsEmailVerified;
            yield return IsPhoneVerified;
        }
    }
} 