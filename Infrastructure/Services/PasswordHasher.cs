using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    /// <summary>
    /// کلاس هش کردن رمز عبور
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int KeySize = 32; // 256 bits
        private const int Iterations = 100000;
        private const char Delimiter = ':';
        private static readonly HashAlgorithmName HashAlgorithmName = HashAlgorithmName.SHA512;

        /// <summary>
        /// هش کردن رمز عبور
        /// </summary>
        public async Task<(string hash, string salt)> HashPasswordAsync(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password
            byte[] hash = await Task.Run(() => KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: Iterations,
                numBytesRequested: KeySize));

            // Combine the salt and hash
            string saltString = Convert.ToBase64String(salt);
            string hashString = Convert.ToBase64String(hash);

            return (hashString, saltString);
        }

        /// <summary>
        /// بررسی تطابق رمز عبور با هش
        /// </summary>
        public async Task<bool> VerifyPasswordAsync(string password, string hash, string salt)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(hash))
                throw new ArgumentNullException(nameof(hash));
            if (string.IsNullOrEmpty(salt))
                throw new ArgumentNullException(nameof(salt));

            // Convert the salt and hash from base64
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] hashBytes = Convert.FromBase64String(hash);

            // Hash the provided password with the same salt
            byte[] computedHash = await Task.Run(() => KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: Iterations,
                numBytesRequested: KeySize));

            // Compare the computed hash with the stored hash
            return CryptographicOperations.FixedTimeEquals(computedHash, hashBytes);
        }

        /// <summary>
        /// بررسی نیاز به بروزرسانی هش
        /// </summary>
        public bool NeedsRehash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                throw new ArgumentNullException(nameof(hash));

            // Check if the hash format is valid
            if (!hash.Contains(Delimiter.ToString()))
                return true;

            // Split the hash into its components
            string[] parts = hash.Split(Delimiter);
            if (parts.Length != 3)
                return true;

            // Check if the algorithm version is current
            if (!int.TryParse(parts[0], out int version) || version != 1)
                return true;

            // Check if the iterations count is current
            if (!int.TryParse(parts[1], out int iterations) || iterations != Iterations)
                return true;

            return false;
        }
    }
} 