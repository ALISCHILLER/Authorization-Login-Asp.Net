using System.Collections.Generic;
using System.Linq;
using Authorization_Login_Asp.Net.Application.DTOs;

namespace Authorization_Login_Asp.Net.Application.Extensions
{
    public static class EnumerableExtensions
    {
        public static List<LoginHistoryItem> ToLoginHistoryItems(this IEnumerable<LoginHistoryDto> loginHistoryDtos)
        {
            return loginHistoryDtos.Select(dto => new LoginHistoryItem
            {
                Id = dto.Id,
                LoginTime = dto.LoginTime,
                IpAddress = dto.IpAddress,
                Location = dto.Location,
                DeviceInfo = dto.DeviceInfo,
                IsSuccessful = dto.IsSuccessful
            }).ToList();
        }
    }
} 