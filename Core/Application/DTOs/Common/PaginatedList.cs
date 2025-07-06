using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Common
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
} 