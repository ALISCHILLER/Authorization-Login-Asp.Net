namespace Authorization_Login_Asp.Net.Core.Domain.Exceptions
{
    /// <summary>
    /// کدهای خطای استاندارد دامنه
    /// </summary>
    public static class DomainErrorCodes
    {
        /// <summary>
        /// خطاهای عمومی
        /// </summary>
        public static class General
        {
            public const string InvalidOperation = "DOMAIN_001";
            public const string InvalidState = "DOMAIN_002";
            public const string ValidationError = "DOMAIN_003";
            public const string NotFound = "DOMAIN_004";
            public const string DuplicateEntry = "DOMAIN_005";
        }

        /// <summary>
        /// خطاهای کاربر
        /// </summary>
        public static class User
        {
            public const string InvalidCredentials = "USER_001";
            public const string AccountLocked = "USER_002";
            public const string AccountDisabled = "USER_003";
            public const string EmailNotVerified = "USER_004";
            public const string PhoneNotVerified = "USER_005";
            public const string InvalidPassword = "USER_006";
            public const string UserNotFound = "USER_007";
            public const string DuplicateUsername = "USER_008";
            public const string DuplicateEmail = "USER_009";
            public const string DuplicatePhone = "USER_010";
        }

        /// <summary>
        /// خطاهای احراز هویت
        /// </summary>
        public static class Authentication
        {
            public const string InvalidToken = "AUTH_001";
            public const string TokenExpired = "AUTH_002";
            public const string InvalidRefreshToken = "AUTH_003";
            public const string TwoFactorRequired = "AUTH_004";
            public const string TwoFactorInvalid = "AUTH_005";
            public const string SessionExpired = "AUTH_006";
            public const string InvalidDevice = "AUTH_007";
        }

        /// <summary>
        /// خطاهای مجوز
        /// </summary>
        public static class Authorization
        {
            public const string InsufficientPermissions = "AUTHZ_001";
            public const string RoleNotFound = "AUTHZ_002";
            public const string PermissionNotFound = "AUTHZ_003";
            public const string InvalidRoleAssignment = "AUTHZ_004";
            public const string InvalidPermissionAssignment = "AUTHZ_005";
        }

        /// <summary>
        /// خطاهای امنیتی
        /// </summary>
        public static class Security
        {
            public const string BruteForceAttempt = "SEC_001";
            public const string SuspiciousActivity = "SEC_002";
            public const string InvalidIpAddress = "SEC_003";
            public const string InvalidUserAgent = "SEC_004";
            public const string SecurityViolation = "SEC_005";
        }
    }
} 