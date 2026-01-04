namespace QT.Systems.Interfaces.Permission;

/// <summary>
/// 业务契约：个人资料管理.
/// </summary>
public interface IUsersCurrentService
{
    Task<string> GetMiniProgramQRCode(string userId);
}