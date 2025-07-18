using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Domain.Interfaces
{
    public interface IUserAuditLogRepository
    {
        Task<IEnumerable<UserAuditLog>> SearchAsync(
            Guid? userId = null,
            string? action = null,
            string? details = null,
            string? ipAddress = null,
            string? userAgent = null,
            bool? isSuccessful = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? count = null,
            CancellationToken cancellationToken = default);

        Task<UserAuditLog> AddLogAsync(
            Guid userId,
            string action,
            string details,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken cancellationToken = default);

        Task<int> CleanupOldLogsAsync(TimeSpan age, CancellationToken cancellationToken = default);
        Task AddAsync(UserAuditLog log, CancellationToken cancellationToken = default);
    }
}
