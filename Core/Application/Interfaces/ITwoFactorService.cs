using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth; // For TwoFactorSetupResponse

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface ITwoFactorService
    {
        /// <summary>
        /// Initiates the setup of two-factor authentication for a user.
        /// Returns data needed for the user to configure their authenticator app (e.g., QR code URI, manual setup key).
        /// </summary>
        Task<TwoFactorSetupResponse> SetupTwoFactorAsync(Guid userId);

        /// <summary>
        /// Verifies the TOTP code during the setup process and enables 2FA for the user if the code is valid.
        /// </summary>
        Task<bool> VerifyTwoFactorSetupAsync(Guid userId, string code);

        /// <summary>
        /// Verifies a TOTP code for a user who already has 2FA enabled (e.g., during login).
        /// </summary>
        Task<bool> VerifyTwoFactorCodeAsync(Guid userId, string code);

        /// <summary>
        /// Validates a TOTP code and returns an authentication response (e.g. JWT tokens) if successful.
        /// Used in the second step of a 2FA login flow.
        /// </summary>
        Task<AuthResponse> ValidateTwoFactorLoginAsync(Guid userId, string code);


        /// <summary>
        /// Disables two-factor authentication for a user after verifying a valid TOTP code.
        /// </summary>
        Task DisableTwoFactorAsync(Guid userId, string code);

        /// <summary>
        /// Generates new backup codes for the user, stores their hashes, and returns the plain text codes.
        /// Typically, these codes are then displayed to the user to be saved securely.
        /// </summary>
        Task<IEnumerable<string>> GenerateAndStoreBackupCodesAsync(Guid userId);

        /// <summary>
        /// Verifies a backup code provided by the user. If valid, the code is consumed (invalidated for future use).
        /// </summary>
        Task<bool> VerifyAndConsumeBackupCodeAsync(Guid userId, string code);

        /// <summary>
        /// Checks if two-factor authentication is currently enabled for the specified user.
        /// </summary>
        Task<bool> IsTwoFactorEnabledAsync(Guid userId);
    }
}
