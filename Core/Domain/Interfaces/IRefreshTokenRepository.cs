namespace Authorization_Login_Asp.Net.Core.Domain.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        // متدهای اختصاصی رفرش توکن در صورت نیاز
    }
}