namespace QT.Common.Net;

/// <summary>
/// UserAgent操作类
/// </summary>
public interface IUserAgent
{
    /// <summary>
    /// 请求客户端是否为移动端
    /// </summary>
    bool isMobileBrowser { get; }

    /// <summary>
    /// 获取设备类型
    /// </summary>
    /// <returns></returns>
    string GetBrowser();

    /// <summary>
    /// 获取操作系统
    /// </summary>
    /// <returns></returns>
    string GetSystem();
}