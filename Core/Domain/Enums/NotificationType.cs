using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Enums
{
    /// <summary>
    /// نوع و اولویت اعلان
    /// </summary>
    [Flags]
    public enum NotificationType
    {
        // انواع اعلان
        None = 0,
        Security = 1 << 0,
        System = 1 << 1,
        Account = 1 << 2,
        Verification = 1 << 3,
        Password = 1 << 4,
        Device = 1 << 5,
        Role = 1 << 6,
        Permission = 1 << 7,

        // اولویت‌های اعلان
        Low = 1 << 8,
        Normal = 1 << 9,
        High = 1 << 10,
        Critical = 1 << 11,

        // ترکیب نوع و اولویت
        SecurityLow = Security | Low,
        SecurityNormal = Security | Normal,
        SecurityHigh = Security | High,
        SecurityCritical = Security | Critical,

        SystemLow = System | Low,
        SystemNormal = System | Normal,
        SystemHigh = System | High,
        SystemCritical = System | Critical,

        AccountLow = Account | Low,
        AccountNormal = Account | Normal,
        AccountHigh = Account | High,
        AccountCritical = Account | Critical,

        VerificationLow = Verification | Low,
        VerificationNormal = Verification | Normal,
        VerificationHigh = Verification | High,
        VerificationCritical = Verification | Critical,

        PasswordLow = Password | Low,
        PasswordNormal = Password | Normal,
        PasswordHigh = Password | High,
        PasswordCritical = Password | Critical,

        DeviceLow = Device | Low,
        DeviceNormal = Device | Normal,
        DeviceHigh = Device | High,
        DeviceCritical = Device | Critical,

        RoleLow = Role | Low,
        RoleNormal = Role | Normal,
        RoleHigh = Role | High,
        RoleCritical = Role | Critical,

        PermissionLow = Permission | Low,
        PermissionNormal = Permission | Normal,
        PermissionHigh = Permission | High,
        PermissionCritical = Permission | Critical
    }
} 