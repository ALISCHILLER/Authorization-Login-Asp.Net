namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
    }
}
