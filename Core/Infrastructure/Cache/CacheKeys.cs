using System;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Cache
{
    /// <summary>
    /// کلاس مدیریت کلیدهای کش
    /// </summary>
    public static class CacheKeys
    {
        private const string UserPrefix = "user:";
        private const string RolePrefix = "role:";
        private const string PermissionPrefix = "permission:";
        private const string LoginHistoryPrefix = "login:";

        /// <summary>
        /// دریافت کلید کش برای یک موجودیت
        /// </summary>
        public static string GetEntityKey<T>(Guid id) where T : class
        {
            return typeof(T).Name.ToLower() + ":" + id;
        }

        /// <summary>
        /// دریافت کلید کش برای یک موجودیت با نام
        /// </summary>
        public static string GetEntityByNameKey<T>(string name) where T : class
        {
            return typeof(T).Name.ToLower() + ":name:" + name;
        }

        // کلیدهای کش کاربر
        public static string User(Guid id) => $"{UserPrefix}{id}";
        public static string UserByEmail(string email) => $"{UserPrefix}email:{email}";
        public static string UserByUsername(string username) => $"{UserPrefix}username:{username}";
        public static string UserRoles(Guid userId) => $"{UserPrefix}{userId}:roles";
        public static string UserPermissions(Guid userId) => $"{UserPrefix}{userId}:permissions";
        public static string UserLoginHistory(Guid userId) => $"{UserPrefix}{userId}:login-history";
        public static string UserFailedLoginCount(Guid userId) => $"{UserPrefix}{userId}:failed-login-count";
        public static string AllUsers() => $"{UserPrefix}all";
        public static string ActiveUsers() => $"{UserPrefix}active";
        public static string LockedUsers() => $"{UserPrefix}locked";

        // کلیدهای کش نقش
        public static string Role(Guid id) => $"{RolePrefix}{id}";
        public static string RoleByName(string name) => $"{RolePrefix}name:{name}";
        public static string RolePermissions(Guid roleId) => $"{RolePrefix}{roleId}:permissions";
        public static string AllRoles() => $"{RolePrefix}all";
        public static string ActiveRoles() => $"{RolePrefix}active";
        public static string SystemRoles() => $"{RolePrefix}system";
        public static string UserRoles() => $"{RolePrefix}user";

        // کلیدهای کش دسترسی
        public static string Permission(Guid id) => $"{PermissionPrefix}{id}";
        public static string PermissionByName(string name) => $"{PermissionPrefix}name:{name}";
        public static string AllPermissions() => $"{PermissionPrefix}all";
        public static string ActivePermissions() => $"{PermissionPrefix}active";
        public static string PermissionGroup(string group) => $"{PermissionPrefix}group:{group}";

        // کلیدهای کش تاریخچه ورود
        public static string LoginHistory(Guid id) => $"{LoginHistoryPrefix}{id}";
        public static string UserLastLogin(Guid userId) => $"{LoginHistoryPrefix}{userId}:last";
        public static string UserLoginCount(Guid userId) => $"{LoginHistoryPrefix}{userId}:count";
        public static string FailedLoginCount(Guid userId) => $"{LoginHistoryPrefix}{userId}:failed-count";
    }
} 