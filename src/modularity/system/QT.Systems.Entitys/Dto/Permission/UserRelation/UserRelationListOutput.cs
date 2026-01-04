using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.UserRelation;

/// <summary>
/// 用户关系列表.
/// </summary>
[SuppressSniffer]
public class UserRelationListOutput
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 用户id.
    /// </summary>
    public string id { get; set; }
}
