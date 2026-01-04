using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.UsersCurrent;

/// <summary>
/// 当前用户系统主题.
/// </summary>
[SuppressSniffer]
public class UsersCurrentSysTheme
{
    /// <summary>
    /// 主题.
    /// </summary>
    public string theme { get; set; }
}