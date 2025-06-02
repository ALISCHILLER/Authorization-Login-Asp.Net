using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Application.Filters
{
    /// <summary>
    /// فیلتر برای بررسی دسترسی‌های سفارشی
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class CustomAuthorizationAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permission;
        private readonly RoleType[] _allowedRoles;

        public CustomAuthorizationAttribute(string permission = null, params RoleType[] allowedRoles)
        {
            _permission = permission;
            _allowedRoles = allowedRoles;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<CustomAuthorizationAttribute>)) as ILogger<CustomAuthorizationAttribute>;
            var permissionService = context.HttpContext.RequestServices.GetService(typeof(IPermissionService)) as IPermissionService;

            if (permissionService == null)
            {
                logger?.LogError("PermissionService not found in DI container");
                context.Result = new StatusCodeResult(500);
                return;
            }

            var userId = context.HttpContext.User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // بررسی نقش‌ها
            if (_allowedRoles != null && _allowedRoles.Length > 0)
            {
                var hasRole = await permissionService.HasAnyRoleAsync(Guid.Parse(userId), _allowedRoles);
                if (!hasRole)
                {
                    logger?.LogWarning("User {UserId} does not have required roles", userId);
                    context.Result = new ForbidResult();
                    return;
                }
            }

            // بررسی دسترسی
            if (!string.IsNullOrEmpty(_permission))
            {
                var hasPermission = await permissionService.HasPermissionAsync(Guid.Parse(userId), _permission);
                if (!hasPermission)
                {
                    logger?.LogWarning("User {UserId} does not have permission {Permission}", userId, _permission);
                    context.Result = new ForbidResult();
                    return;
                }
            }
        }
    }
} 