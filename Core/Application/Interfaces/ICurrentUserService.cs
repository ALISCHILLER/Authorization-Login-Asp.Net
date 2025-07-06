namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        string UserName { get; }
    }
}
