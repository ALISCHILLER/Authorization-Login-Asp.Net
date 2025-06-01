using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Presentation.Api.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ILogger<PermissionHandler> _logger;

        public PermissionHandler(ILogger<PermissionHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            try
            {
                var user = context.User;
                if (!user.Identity.IsAuthenticated)
                {
                    _logger.LogWarning("Unauthorized access attempt: User is not authenticated");
                    return Task.CompletedTask;
                }

                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized access attempt: User ID not found in claims");
                    return Task.CompletedTask;
                }

                // بررسی مجوزهای کاربر
                var permissions = user.Claims
                    .Where(c => c.Type == "permission")
                    .Select(c => c.Value)
                    .ToList();

                if (permissions.Contains(requirement.Permission))
                {
                    _logger.LogInformation("User {UserId} has permission {Permission}", userId, requirement.Permission);
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogWarning(
                        "Unauthorized access attempt: User {UserId} does not have permission {Permission}",
                        userId,
                        requirement.Permission);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking permissions");
            }

            return Task.CompletedTask;
        }
    }
}