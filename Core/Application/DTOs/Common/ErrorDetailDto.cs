using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Common
{
    public class ErrorDetailDto
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string? Code { get; set; } // Optional error code, can be used for specific client-side handling
        public DateTime ErrorTime { get; set; } // Will be set by ErrorHandlingService using IDateTimeService
        public IDictionary<string, object>? AdditionalData { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; } // Specifically for validation errors
    }
}
