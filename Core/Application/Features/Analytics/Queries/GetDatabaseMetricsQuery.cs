namespace Authorization_Login_Asp.Net.Core.Application.Features.Analytics.Queries;

using Authorization_Login_Asp.Net.Core.Application.DTOs.Analytics;

public record GetDatabaseMetricsQuery : MediatR.IRequest<GetDatabaseMetricsResponse>;
