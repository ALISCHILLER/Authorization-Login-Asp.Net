using System;
using System.Collections.Generic;
using System.Linq;
using Authorization_Login_Asp.Net.Domain.Entities;

namespace Authorization_Login_Asp.Net.Domain.Common
{
    public class LoginHistoryResult
    {
        public List<LoginHistory> Items { get; set; }
        public int TotalCount { get; set; }

        public LoginHistoryResult(IEnumerable<LoginHistory> items, int totalCount)
        {
            Items = items.ToList();
            TotalCount = totalCount;
        }

        public IEnumerable<TResult> Select<TResult>(Func<LoginHistory, TResult> selector)
        {
            return Items.Select(selector);
        }
    }
} 