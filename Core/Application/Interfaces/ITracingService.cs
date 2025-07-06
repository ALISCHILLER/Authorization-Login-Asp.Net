using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface ITracingService
    {
        ActivitySource CreateActivitySource(string name);
        void AddTracing(IServiceCollection services);
        Activity StartActivity(string name, ActivityKind kind = ActivityKind.Internal, ActivityContext? parentContext = null, IEnumerable<KeyValuePair<string, object>>? attributes = null);
        void AddEvent(string name, params (string key, object value)[] attributes);
        Task<T> ExecuteInActivityAsync<T>(string name, Func<Task<T>> operation, ActivityKind kind = ActivityKind.Internal, IEnumerable<KeyValuePair<string, object>>? attributes = null);
        Task ExecuteInActivityAsync(string name, Func<Task> operation, ActivityKind kind = ActivityKind.Internal, IEnumerable<KeyValuePair<string, object>>? attributes = null);
        Task StartTraceAsync(string operationName, string correlationId = null);
        Task EndTraceAsync(string operationName, string correlationId = null);
        Task AddTraceAttributeAsync(string operationName, string key, string value, string correlationId = null);
        Task AddTraceEventAsync(string operationName, string eventName, string correlationId = null);
        Task<string> GetCorrelationIdAsync();
        Task SetCorrelationIdAsync(string correlationId);
        Task<bool> IsTraceEnabledAsync();
        Task EnableTraceAsync();
        Task DisableTraceAsync();
        Task<TimeSpan> GetOperationDurationAsync(string operationName, string correlationId = null);
        Task<IEnumerable<string>> GetActiveOperationsAsync();
    }
}