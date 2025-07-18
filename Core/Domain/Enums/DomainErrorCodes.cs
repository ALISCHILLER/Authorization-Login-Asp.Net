namespace Authorization_Login_Asp.Net.Core.Domain.Enums
{
    public static class DomainErrorCodes
    {
        public static class General
        {
            public const string NotFound = "DOMAIN.GENERAL.NOT_FOUND";
            public const string ValidationError = "DOMAIN.GENERAL.VALIDATION_ERROR";
            public const string InvalidOperation = "DOMAIN.GENERAL.INVALID_OPERATION";
            public const string DuplicateEntry = "DOMAIN.GENERAL.DUPLICATE_ENTRY";
            public const string UnauthorizedAccess = "DOMAIN.GENERAL.UNAUTHORIZED_ACCESS";
        }
        public static class User
        {
            public const string UserNotFound = "DOMAIN.USER.NOT_FOUND";
            public const string UserLocked = "DOMAIN.USER.LOCKED";
            public const string UserAlreadyExists = "DOMAIN.USER.ALREADY_EXISTS";
            public const string InvalidPassword = "DOMAIN.USER.INVALID_PASSWORD";
        }
        public static class Security
        {
            public const string SuspiciousActivity = "DOMAIN.SECURITY.SUSPICIOUS_ACTIVITY";
            public const string AccessDenied = "DOMAIN.SECURITY.ACCESS_DENIED";
        }
    }
}