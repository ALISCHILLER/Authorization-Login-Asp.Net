using System;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// موجودیت تاریخچه تغییرات
    /// </summary>
    public class AuditLog : BaseEntity
    {
        [Required]
        public string Action { get; set; }

        public string EntityName { get; set; }
        public string EntityId { get; set; }

        public string? UserId { get; set; }
        public string? UserName { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        public string? OldValues { get; set; }
        public string? NewValues { get; set; }

        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; }

        [MaxLength(256)]
        public string? UserAgent { get; set; }

        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }

        public string? RequestPath { get; set; }
        public string? RequestMethod { get; set; }
        public int? ResponseStatusCode { get; set; }
        public long? ExecutionTime { get; set; }

        public virtual User? User { get; set; }

        protected AuditLog() { }

        public AuditLog(
            string action,
            string entityName,
            string entityId,
            string userId,
            string userName,
            string ipAddress,
            string userAgent,
            string? oldValues = null,
            string? newValues = null)
        {
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action cannot be empty", nameof(action));
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentException("EntityName cannot be empty", nameof(entityName));

            Id = Guid.NewGuid();
            Action = action;
            EntityName = entityName;
            EntityId = entityId;
            UserId = userId;
            UserName = userName;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            OldValues = oldValues;
            NewValues = newValues;
            Timestamp = DateTime.UtcNow;
            Succeeded = true;
        }

        public void MarkAsFailed(string errorMessage)
        {
            Succeeded = false;
            ErrorMessage = errorMessage;
        }

        public void SetResponseDetails(int statusCode, long executionTime)
        {
            ResponseStatusCode = statusCode;
            ExecutionTime = executionTime;
        }

        public void SetRequestDetails(string path, string method)
        {
            RequestPath = path;
            RequestMethod = method;
        }
    }
}