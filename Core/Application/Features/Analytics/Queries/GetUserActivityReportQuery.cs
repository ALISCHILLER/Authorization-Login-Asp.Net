using System;
namespace Authorization_Login_Asp.Net.Core.Application.Features.Analytics.Queries;

using Authorization_Login_Asp.Net.Core.Application.DTOs.Analytics;

public record GetUserActivityReportQuery : MediatR.IRequest<GetUserActivityReportResponse>
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
