using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.User;

/// <summary>
/// 更新用户输入
/// </summary>
[SuppressSniffer]
public class UserUpInput : UserCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}