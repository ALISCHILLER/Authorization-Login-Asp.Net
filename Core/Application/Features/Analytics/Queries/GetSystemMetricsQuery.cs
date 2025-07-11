namespace Authorization_Login_Asp.Net.Core.Application.Features.Analytics.Queries;

// Assuming GetSystemMetricsResponse will be in the DTOs.Analytics namespace
using Authorization_Login_Asp.Net.Core.Application.DTOs.Analytics;

public record GetSystemMetricsQuery : MediatR.IRequest<GetSystemMetricsResponse>;
