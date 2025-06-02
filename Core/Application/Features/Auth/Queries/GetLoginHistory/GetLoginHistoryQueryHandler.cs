using Authorization_Login_Asp.Net.Core.Application.Common.Interfaces;
using Authorization_Login_Asp.Net.Core.Application.Common.Mappings;
using Authorization_Login_Asp.Net.Core.Application.Exceptions;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Core.Application.Features.Auth.Queries.GetLoginHistory
{
    /// <summary>
    /// پردازشگر درخواست دریافت تاریخچه ورود
    /// </summary>
    public class GetLoginHistoryQueryHandler : IRequestHandler<GetLoginHistoryQuery, GetLoginHistoryResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GetLoginHistoryQueryHandler> _logger;

        public GetLoginHistoryQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<GetLoginHistoryQueryHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GetLoginHistoryResponse> Handle(GetLoginHistoryQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // بررسی وجود کاربر
                var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
                if (!userExists)
                {
                    throw new NotFoundException($"کاربر با شناسه {request.UserId} یافت نشد");
                }

                // ایجاد کوئری پایه
                var query = _context.LoginHistories
                    .Include(lh => lh.User)
                    .Where(lh => lh.UserId == request.UserId);

                // اعمال فیلترها
                if (request.IsSuccess.HasValue)
                {
                    query = query.Where(lh => lh.IsSuccess == request.IsSuccess.Value);
                }

                if (request.FromDate.HasValue)
                {
                    query = query.Where(lh => lh.CreatedAt >= request.FromDate.Value);
                }

                if (request.ToDate.HasValue)
                {
                    query = query.Where(lh => lh.CreatedAt <= request.ToDate.Value);
                }

                // محاسبه تعداد کل رکوردها
                var totalCount = await query.CountAsync(cancellationToken);

                // محاسبه تعداد صفحات
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                // دریافت رکوردها با صفحه‌بندی
                var items = await query
                    .OrderByDescending(lh => lh.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                // تبدیل به DTO
                var itemDtos = _mapper.Map<ICollection<LoginHistoryDto>>(items);

                return new GetLoginHistoryResponse
                {
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < totalPages,
                    Items = itemDtos
                };
            }
            catch (Exception ex) when (ex is not NotFoundException)
            {
                _logger.LogError(ex, "خطا در دریافت تاریخچه ورود برای کاربر {UserId}", request.UserId);
                throw;
            }
        }
    }
} 