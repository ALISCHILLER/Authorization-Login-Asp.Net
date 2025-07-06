using System;
using System.Threading.Tasks;
using System.Text.Json;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;
        private readonly ICurrentUserService _currentUserService;

        public AuditService(
            ApplicationDbContext context,
            ILogger<AuditService> logger,
            ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task LogActionAsync(
            string action,
            string entityName,
            string entityId,
            string oldValues,
            string newValues,
            bool succeeded,
            string errorMessage = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = _currentUserService.UserId,
                    UserName = _currentUserService.UserName,
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    OldValues = oldValues,
                    NewValues = newValues,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = _currentUserService.GetIpAddress(),
                    UserAgent = _currentUserService.GetUserAgent(),
                    Succeeded = succeeded,
                    ErrorMessage = errorMessage
                };

                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Audit log created for {Action} on {EntityName} {EntityId} by {UserName}",
                    action, entityName, entityId, _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit log for {Action} on {EntityName}", action, entityName);
                throw;
            }
        }

        public async Task LogAuthenticationAsync(
            string action,
            string username,
            bool succeeded,
            string ipAddress,
            string userAgent,
            string errorMessage = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Action = action,
                    UserName = username,
                    EntityName = "Authentication",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Succeeded = succeeded,
                    ErrorMessage = errorMessage
                };

                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Authentication {Action} for user {UserName} from {IpAddress} - {Succeeded}",
                    action, username, ipAddress, succeeded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating authentication audit log for user {UserName}", username);
                throw;
            }
        }

        public async Task LogAuthorizationAsync(
            string action,
            string resource,
            bool succeeded,
            string errorMessage = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = _currentUserService.UserId,
                    UserName = _currentUserService.UserName,
                    Action = action,
                    EntityName = "Authorization",
                    EntityId = resource,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = _currentUserService.GetIpAddress(),
                    UserAgent = _currentUserService.GetUserAgent(),
                    Succeeded = succeeded,
                    ErrorMessage = errorMessage
                };

                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Authorization {Action} for user {UserName} on {Resource} - {Succeeded}",
                    action, _currentUserService.UserName, resource, succeeded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating authorization audit log for {Resource}", resource);
                throw;
            }
        }

        public async Task LogSystemEventAsync(
            string eventType,
            string description,
            string source,
            bool succeeded,
            string errorMessage = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Action = eventType,
                    EntityName = "SystemEvent",
                    EntityId = source,
                    NewValues = description,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = _currentUserService.GetIpAddress(),
                    UserAgent = _currentUserService.GetUserAgent(),
                    Succeeded = succeeded,
                    ErrorMessage = errorMessage
                };

                if (_currentUserService.IsAuthenticated)
                {
                    auditLog.UserId = _currentUserService.UserId;
                    auditLog.UserName = _currentUserService.UserName;
                }

                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "System event {EventType} from {Source}: {Description}",
                    eventType, source, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating system event audit log for {EventType}", eventType);
                throw;
            }
        }
    }
}