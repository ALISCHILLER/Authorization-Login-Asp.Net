using System;
using System.Collections.Generic;
using System.Linq;
using Authorization_Login_Asp.Net.Domain.Entities;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    public class LoginHistoryResult
    {
        public List<LoginHistory> Items { get; set; }
        public int TotalCount { get; set; }

        public IEnumerable<TResult> Select<TResult>(Func<LoginHistory, TResult> selector)
        {
            return Items.Select(selector);
        }

        public LoginHistoryResult()
        {
            Items = new List<LoginHistory>();
            TotalCount = 0;
        }

        public LoginHistoryResult(List<LoginHistory> items, int totalCount)
        {
            Items = items ?? new List<LoginHistory>();
            TotalCount = totalCount;
        }
    }
} 