using Authorization_Login_Asp.Net.Core.Application.Common.Mappings;
using MediatR;

namespace Authorization_Login_Asp.Net.Core.Application.Features.Auth.Queries.GetLoginHistory
{
    /// <summary>
    /// درخواست دریافت تاریخچه ورود
    /// </summary>
    public class GetLoginHistoryQuery : IRequest<GetLoginHistoryResponse>
    {
        public int UserId { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public bool? IsSuccess { get; }
        public DateTime? FromDate { get; }
        public DateTime? ToDate { get; }

        public GetLoginHistoryQuery(
            int userId,
            int pageNumber = 1,
            int pageSize = 10,
            bool? isSuccess = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            UserId = userId;
            PageNumber = pageNumber;
            PageSize = pageSize;
            IsSuccess = isSuccess;
            FromDate = fromDate;
            ToDate = toDate;
        }
    }

    /// <summary>
    /// پاسخ درخواست تاریخچه ورود
    /// </summary>
    public class GetLoginHistoryResponse
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public ICollection<LoginHistoryDto> Items { get; set; } = new List<LoginHistoryDto>();
    }
} 