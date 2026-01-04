using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.UsersCurrent;

/// <summary>
/// 用户切换3个默认 输入.
/// </summary>
[SuppressSniffer]
public class UsersCurrentDefaultOrganizeInput
{
    /// <summary>
    /// 默认切换类型：Organize：组织，Position：岗位：Role：角色.
    /// </summary>
    public string majorType { get; set; }

    /// <summary>
    /// 默认切换Id（组织Id、岗位Id、角色Id）.
    /// </summary>
    public string majorId { get; set; }
}